using Project05.Component;
using Project05.Component.Data;
using Project05.Component.Input;
using Project05.Foundation;
using Project05.UI.View;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Project05.UI.Presenter
{
    /// <summary>
    /// [Game] 일시정지 사유가 BuildMenu일 때(공터에서 Space) 건설 목록을 띄운다. Runner가 LateUpdate에서 매 프레임 호출한다.
    /// 해금된 종류만 보이고, 골드가 모자란 종류는 버튼을 막는다. 버튼은 World를 바꾸지 않고 BuildTower·CloseBuildMenu 명령만 넣는다.
    /// </summary>
    public class BuildMenuPresenter
    {
        private readonly BuildMenuView _view;
        private readonly World _world;
        private readonly BuildMenuEntryView[] _entries;
        private bool _isShown;

        // 비용 문구 뒤에 붙이는 골드 단위
        private const string GOLD_SUFFIX = "G";

        /// <summary>
        /// 건설 목록 View와 이번 판의 월드를 받아 타워 종류 수만큼 칸을 만들고, 아이콘·비용을 채우고, 버튼을 연결한다.
        /// </summary>
        /// <param name="view">씬의 건설 목록 View</param>
        /// <param name="world">이번 판의 월드 (버튼 명령을 넣을 곳)</param>
        public BuildMenuPresenter(BuildMenuView view, World world)
        {
            _view = view;
            _world = world;
            _view.SetVisible(false);

            TowerStats[] towerStats = world.towerStats;
            _entries = _view.CreateEntries(towerStats.Length);
            for (int towerTypeIndex = 0; towerTypeIndex < _entries.Length; towerTypeIndex++)
            {
                BuildMenuEntryView entry = _entries[towerTypeIndex];
                // 판 시작 시 한 번 만드는 문자열이라 할당은 문제없다
                entry.SetContent(GetIcon(towerTypeIndex), GetName(towerTypeIndex), towerStats[towerTypeIndex].buildCost + GOLD_SUFFIX);
                if (entry.BuildButton != null)
                {
                    entry.BuildButton.onClick.AddListener(CreateBuildHandler(towerTypeIndex));
                }
            }

            if (_view.CloseButton != null)
            {
                _view.CloseButton.onClick.AddListener(EnqueueClose);
            }
        }

        /// <summary>
        /// 건설 목록이 열려 있으면 패널을 띄우고 칸마다 해금·골드 상태를 맞춘다. 아니면 숨긴다.
        /// </summary>
        /// <param name="world">현재 판의 월드 (읽기 전용)</param>
        public void Present(World world)
        {
            bool isOpen = world.pauseReason == PauseReason.BuildMenu;
            if (isOpen != _isShown)
            {
                _isShown = isOpen;
                _view.SetVisible(isOpen);
            }
            if (!isOpen)
            {
                return;
            }

            TowerStats[] towerStats = world.towerStats;
            bool[] isUnlockedByTowerType = world.towerUnlock.isUnlockedByTowerType;
            int gold = world.progress.gold;
            for (int towerTypeIndex = 0; towerTypeIndex < _entries.Length; towerTypeIndex++)
            {
                ref TowerStats stats = ref towerStats[towerTypeIndex];
                bool isBuildable = towerTypeIndex < isUnlockedByTowerType.Length && isUnlockedByTowerType[towerTypeIndex] && stats.levels.Length > 0;
                _entries[towerTypeIndex].SetVisible(isBuildable);
                _entries[towerTypeIndex].SetBuildInteractable(gold >= stats.buildCost);
            }
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
        /// 칸마다 자기 타워 종류를 담은 핸들러를 만든다 (판 시작 시 1회).
        /// </summary>
        private UnityAction CreateBuildHandler(int towerTypeIndex)
        {
            return () => EnqueueBuild(towerTypeIndex);
        }

        private void EnqueueBuild(int towerTypeIndex)
        {
            Command buildCommand = new Command();
            buildCommand.kind = CommandKind.BuildTower;
            buildCommand.towerTypeIndex = towerTypeIndex;
            _world.input.commands.Enqueue(buildCommand);
        }

        private void EnqueueClose()
        {
            Command closeCommand = new Command();
            closeCommand.kind = CommandKind.CloseBuildMenu;
            _world.input.commands.Enqueue(closeCommand);
        }
    }
}
