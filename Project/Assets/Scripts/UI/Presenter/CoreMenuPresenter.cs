using Project05.Component;
using Project05.Component.Data;
using Project05.Component.Entity;
using Project05.Component.Input;
using Project05.Foundation;
using Project05.UI.View;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Project05.UI.Presenter
{
    /// <summary>
    /// [Game] 일시정지 사유가 CoreMenu일 때(코어 근처에서 Space) 코어 UI를 띄운다 (DESIGN 3.2). Runner가 LateUpdate에서 매 프레임 호출한다.
    /// 해금 탭: 타워 종류별 해금. 타워 탭: 공터별 업그레이드·수리. 탭 전환은 화면 상태라 Presenter가 가진다.
    /// 버튼은 World를 바꾸지 않고 UnlockTowerType·UpgradeTower·RepairTower·CloseCoreMenu 명령만 넣는다.
    /// 문구는 표시 값(타워·단계·HP)이 바뀐 칸만 다시 만든다.
    /// </summary>
    public class CoreMenuPresenter
    {
        private readonly CoreMenuView _view;
        private readonly World _world;
        private readonly CoreUnlockEntryView[] _unlockEntries;
        private readonly CoreTowerEntryView[] _towerEntries;
        // 타워 칸마다 마지막으로 문구를 만든 값 (바뀌었는지 비교용)
        private readonly EntityID[] _shownTowerIDs;
        private readonly int[] _shownLevels;
        private readonly int[] _shownHPs;
        private bool _isShown;
        private bool _isUnlockTab = true;

        private const string GOLD_SUFFIX = "G";
        private const string EMPTY_SLOT_TEXT = "비어 있음";
        // 표시 값 캐시의 초기값 (어떤 실제 값과도 다르게)
        private const int UNSHOWN_VALUE = int.MinValue;

        /// <summary>
        /// 코어 UI View와 이번 판의 월드를 받아 해금 칸(타워 종류 수)과 타워 칸(공터 수)을 만들고 버튼을 연결한다.
        /// </summary>
        /// <param name="view">씬의 코어 UI View</param>
        /// <param name="world">이번 판의 월드 (버튼 명령을 넣을 곳)</param>
        public CoreMenuPresenter(CoreMenuView view, World world)
        {
            _view = view;
            _world = world;
            _view.SetVisible(false);

            TowerStats[] towerStats = world.towerStats;
            _unlockEntries = _view.CreateUnlockEntries(towerStats.Length);
            for (int towerTypeIndex = 0; towerTypeIndex < _unlockEntries.Length; towerTypeIndex++)
            {
                CoreUnlockEntryView entry = _unlockEntries[towerTypeIndex];
                // 판 시작 시 한 번 만드는 문자열이라 할당은 문제없다
                entry.SetContent(GetIcon(towerTypeIndex), GetName(towerTypeIndex), towerStats[towerTypeIndex].unlockCost + GOLD_SUFFIX);
                if (entry.UnlockButton != null)
                {
                    entry.UnlockButton.onClick.AddListener(CreateCommandHandler(CommandKind.UnlockTowerType, towerTypeIndex));
                }
            }

            int slotCount = world.towerSlots.Length;
            _towerEntries = _view.CreateTowerEntries(slotCount);
            _shownTowerIDs = new EntityID[_towerEntries.Length];
            _shownLevels = new int[_towerEntries.Length];
            _shownHPs = new int[_towerEntries.Length];
            for (int slotIndex = 0; slotIndex < _towerEntries.Length; slotIndex++)
            {
                CoreTowerEntryView entry = _towerEntries[slotIndex];
                if (entry.UpgradeButton != null)
                {
                    entry.UpgradeButton.onClick.AddListener(CreateCommandHandler(CommandKind.UpgradeTower, slotIndex));
                }
                if (entry.RepairButton != null)
                {
                    entry.RepairButton.onClick.AddListener(CreateCommandHandler(CommandKind.RepairTower, slotIndex));
                }
            }

            if (_view.UnlockTabButton != null)
            {
                _view.UnlockTabButton.onClick.AddListener(ShowUnlockTab);
            }
            if (_view.TowerTabButton != null)
            {
                _view.TowerTabButton.onClick.AddListener(ShowTowerTab);
            }
            if (_view.CloseButton != null)
            {
                _view.CloseButton.onClick.AddListener(CreateCommandHandler(CommandKind.CloseCoreMenu, 0));
            }
        }

        /// <summary>
        /// 코어 UI가 열려 있으면 패널을 띄우고 두 탭의 칸을 맞춘다. 아니면 숨긴다. 열릴 때마다 해금 탭부터 보인다.
        /// </summary>
        /// <param name="world">현재 판의 월드 (읽기 전용)</param>
        public void Present(World world)
        {
            bool isOpen = world.pauseReason == PauseReason.CoreMenu;
            if (isOpen != _isShown)
            {
                _isShown = isOpen;
                _view.SetVisible(isOpen);
                if (isOpen)
                {
                    _isUnlockTab = true;
                    _view.ShowTab(_isUnlockTab);
                    ResetShownValues();
                }
            }
            if (!isOpen)
            {
                return;
            }

            int gold = world.progress.gold;
            PresentUnlockEntries(world, gold);
            PresentTowerEntries(world, gold);
        }

        private void PresentUnlockEntries(World world, int gold)
        {
            bool[] isUnlockedByTowerType = world.towerUnlock.isUnlockedByTowerType;
            for (int towerTypeIndex = 0; towerTypeIndex < _unlockEntries.Length; towerTypeIndex++)
            {
                bool isUnlocked = towerTypeIndex < isUnlockedByTowerType.Length && isUnlockedByTowerType[towerTypeIndex];
                _unlockEntries[towerTypeIndex].SetState(isUnlocked, gold >= world.towerStats[towerTypeIndex].unlockCost);
            }
        }

        /// <summary>
        /// 공터마다 상태 문구와 업그레이드·수리 버튼을 맞춘다. 문구는 타워·단계·HP가 바뀐 칸만 다시 만든다.
        /// </summary>
        private void PresentTowerEntries(World world, int gold)
        {
            for (int slotIndex = 0; slotIndex < _towerEntries.Length; slotIndex++)
            {
                CoreTowerEntryView entry = _towerEntries[slotIndex];
                EntityID towerID = world.towerSlots[slotIndex].towerID;
                if (!world.towers.IsAlive(towerID))
                {
                    if (_shownTowerIDs[slotIndex].IsValid || _shownLevels[slotIndex] == UNSHOWN_VALUE)
                    {
                        _shownTowerIDs[slotIndex] = default(EntityID);
                        _shownLevels[slotIndex] = 0;
                        _shownHPs[slotIndex] = 0;
                        entry.SetInfoText((slotIndex + 1) + ". " + EMPTY_SLOT_TEXT);
                    }
                    entry.SetUpgrade(false, false, null);
                    entry.SetRepair(false, false, null);
                    continue;
                }

                ref Tower tower = ref world.towers.Get(towerID);
                ref TowerStats stats = ref world.towerStats[tower.towerTypeIndex];
                bool isMaxLevel = tower.level >= stats.levels.Length - 1;
                int upgradeCost = isMaxLevel ? 0 : stats.levels[tower.level].upgradeCost;

                bool hasChanged = _shownTowerIDs[slotIndex] != towerID || _shownLevels[slotIndex] != tower.level || _shownHPs[slotIndex] != tower.currentHP;
                string upgradeCostText = null;
                string repairCostText = null;
                if (hasChanged)
                {
                    bool isNewTower = _shownTowerIDs[slotIndex] != towerID || _shownLevels[slotIndex] != tower.level;
                    _shownTowerIDs[slotIndex] = towerID;
                    _shownLevels[slotIndex] = tower.level;
                    _shownHPs[slotIndex] = tower.currentHP;
                    entry.SetInfoText((slotIndex + 1) + ". " + GetName(tower.towerTypeIndex) + " Lv." + (tower.level + 1) + "  HP " + tower.currentHP + "/" + tower.maxHP);
                    if (isNewTower)
                    {
                        upgradeCostText = upgradeCost + GOLD_SUFFIX;
                        repairCostText = stats.repairCost + GOLD_SUFFIX;
                    }
                }

                entry.SetUpgrade(!isMaxLevel, gold >= upgradeCost, upgradeCostText);
                entry.SetRepair(true, tower.currentHP < tower.maxHP && gold >= stats.repairCost, repairCostText);
            }
        }

        private void ResetShownValues()
        {
            for (int slotIndex = 0; slotIndex < _towerEntries.Length; slotIndex++)
            {
                _shownTowerIDs[slotIndex] = default(EntityID);
                _shownLevels[slotIndex] = UNSHOWN_VALUE;
                _shownHPs[slotIndex] = UNSHOWN_VALUE;
            }
        }

        private void ShowUnlockTab()
        {
            _isUnlockTab = true;
            _view.ShowTab(_isUnlockTab);
        }

        private void ShowTowerTab()
        {
            _isUnlockTab = false;
            _view.ShowTab(_isUnlockTab);
        }

        private static Sprite GetIcon(int towerTypeIndex)
        {
            Sprite[] towerIcons = App.ViewData.towerIcons;
            return towerTypeIndex < towerIcons.Length ? towerIcons[towerTypeIndex] : null;
        }

        private static string GetName(int towerTypeIndex)
        {
            string[] towerNames = App.ViewData.towerNames;
            return towerTypeIndex < towerNames.Length ? towerNames[towerTypeIndex] : string.Empty;
        }

        /// <summary>
        /// 버튼마다 명령 종류와 대상 번호(타워 종류 또는 공터)를 담은 핸들러를 만든다 (판 시작 시 1회).
        /// </summary>
        private UnityAction CreateCommandHandler(CommandKind kind, int targetIndex)
        {
            return () => EnqueueCommand(kind, targetIndex);
        }

        private void EnqueueCommand(CommandKind kind, int targetIndex)
        {
            Command command = new Command();
            command.kind = kind;
            command.towerTypeIndex = targetIndex;
            command.slotIndex = targetIndex;
            _world.input.commands.Enqueue(command);
        }
    }
}
