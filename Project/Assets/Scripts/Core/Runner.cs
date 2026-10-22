using Project05.Component;
using Project05.Component.Data;
using Project05.Component.Entity;
using Project05.Component.Input;
using Project05.Core.Baking;
using Project05.Foundation;
using Project05.System;
using Project05.UI.Presenter;
using Project05.UI.View;
using Project05.View;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project05.Core
{
    /// <summary>
    /// DDOL에 올라가는 유일한 오브젝트. 현재 판의 World와 ViewContext를 보관하고 루프를 돌린다.
    /// FixedUpdate: System Tick / Update: 입력 수집 / LateUpdate: 패배 시 기록 저장 → ViewSystem → Presenter → 프레임 기록 비우기.
    /// Main 씬에서는 World 없이 메인 메뉴·설정·로딩 Presenter만 둔다.
    /// 실행 순서는 기본값(0)으로 두어 CinemachineBrain(100)보다 먼저 LateUpdate를 돈다.
    /// </summary>
    public class Runner : MonoBehaviour
    {
        private readonly InputCollector _inputCollector = new InputCollector();
        private World _world;
        private ViewContext _viewContext;
        // 결과 패널 Presenter (씬에 ResultView가 없으면 null)
        private ResultPresenter _resultPresenter;
        // 레벨업 선택지 Presenter (씬에 LevelUpChoiceView가 없으면 null)
        private LevelUpChoicePresenter _levelUpChoicePresenter;
        // 건설 목록 Presenter (씬에 BuildMenuView가 없으면 null)
        private BuildMenuPresenter _buildMenuPresenter;
        // 코어 UI Presenter (씬에 CoreMenuView가 없으면 null)
        private CoreMenuPresenter _coreMenuPresenter;
        // 일시정지 메뉴 Presenter (씬에 PauseMenuView가 없으면 null)
        private PauseMenuPresenter _pauseMenuPresenter;
        // 이번 판의 기록을 저장했는지 (패배 감지 시 한 번만 저장)
        private bool _hasSavedRecord;
        // [Main] 메인 메뉴 Presenter (Main 씬 밖이거나 View가 없으면 null)
        private MainMenuPresenter _mainMenuPresenter;
        // [Main] 로딩 패널 Presenter (매 프레임 진행률 갱신, 없으면 null)
        private LoadingPresenter _loadingPresenter;
        private int _activeSceneHandle;
        private bool _hasActiveScene;

        private const float DEBUG_PANEL_MARGIN = 10f;
        private const float DEBUG_PANEL_WIDTH = 420f;
        private const float DEBUG_PANEL_HEIGHT = 820f;
        // 레인별 테스트 씬 폴더 (CONVENTIONS 7.5). Build Settings에 넣지 않으므로 에디터에서만 판이 시작된다
        private const string TEST_SCENE_FOLDER = "Assets/Scenes/Test/";

        /// <summary>
        /// 현재 판의 월드. Game 씬 밖에서는 null.
        /// </summary>
        public World World
        {
            get { return _world; }
        }

        /// <summary>
        /// 현재 판의 화면 상태. Game 씬 밖에서는 null.
        /// </summary>
        public ViewContext ViewContext
        {
            get { return _viewContext; }
        }

        private void FixedUpdate()
        {
            SynchronizeSession();
            if (_world == null)
            {
                return;
            }

            // 패배 시 CommandSystem을 포함한 모든 System을 멈춘다
            if (!SessionSystem.IsCommandProcessing(_world))
            {
                return;
            }

            // [Tick 1] 일시정지 중에도 실행 (열린 UI의 명령만 처리)
            CommandSystem.Tick(_world);

            // 일시정지 중이면 CommandSystem만 실행한다. 이번 틱에 패배·일시정지가 됐어도 여기서 멈춘다
            if (!SessionSystem.IsSimulationRunning(_world))
            {
                return;
            }

            // [Tick 2 ~ 9] DESIGN 5.5 순서 (ProjectileSystem은 TowerSystem과 EnemySystem 사이). SpawnSystem은 반드시 마지막
            GameFlowSystem.Tick(_world);
            WaveSystem.Tick(_world);
            HeroSystem.Tick(_world);
            TowerSystem.Tick(_world);
            ProjectileSystem.Tick(_world);
            EnemySystem.Tick(_world);
            DropSystem.Tick(_world);
            CoreSystem.Tick(_world);
            SpawnSystem.Tick(_world);

            SessionSystem.AdvanceTick(_world);
        }

        private void Update()
        {
            SynchronizeSession();
            if (_world == null)
            {
                return;
            }

            _inputCollector.Collect(_world);
        }

        private void LateUpdate()
        {
            SynchronizeSession();
            if (_world == null)
            {
                // Main 씬: 로딩 진행률만 매 프레임 갱신한다
                if (_loadingPresenter != null)
                {
                    _loadingPresenter.Present();
                }
                return;
            }

            SaveRecordOnDefeat();

            // ① ViewSystem: 엔티티 동기화·보간 → 연출. 카메라는 히어로 보간 이후
            HeroViewSystem.Present(_world, _viewContext);
            TowerViewSystem.Present(_world, _viewContext);
            EnemyViewSystem.Present(_world, _viewContext);
            ProjectileViewSystem.Present(_world, _viewContext);
            DropViewSystem.Present(_world, _viewContext);
            CoreViewSystem.Present(_world, _viewContext);
            InteractionPromptViewSystem.Present(_world, _viewContext);
            OffscreenIndicatorViewSystem.Present(_world, _viewContext);
            CameraViewSystem.Present(_world, _viewContext);

            // ② Presenter 호출. 판 시작 시 씬에서 찾은 View로 만든 Presenter를 정해진 순서로 부른다
            if (_buildMenuPresenter != null)
            {
                _buildMenuPresenter.Present(_world);
            }
            if (_coreMenuPresenter != null)
            {
                _coreMenuPresenter.Present(_world);
            }
            if (_pauseMenuPresenter != null)
            {
                _pauseMenuPresenter.Present(_world);
            }
            if (_levelUpChoicePresenter != null)
            {
                _levelUpChoicePresenter.Present(_world);
            }
            if (_resultPresenter != null)
            {
                _resultPresenter.Present(_world);
            }

            // ③ 이번 프레임의 기록은 모두 연출됐으므로 비운다
            SessionSystem.ClearFrameRecords(_world);
        }

        /// <summary>
        /// 활성 씬이 바뀌었는지 매 루프 시작 시 확인해 판을 시작하거나 끝낸다.
        /// 이벤트 사용 금지 규칙에 따라 SceneManager 이벤트를 구독하지 않고 활성 씬 handle을 비교한다.
        /// Game 씬 재로드(재시작)도 handle이 바뀌므로 새 판으로 처리된다. 테스트 씬도 판을 시작한다.
        /// </summary>
        private void SynchronizeSession()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (_hasActiveScene && activeScene.handle == _activeSceneHandle)
            {
                return;
            }

            _hasActiveScene = true;
            _activeSceneHandle = activeScene.handle;
            EndSession();

            if (IsGameplayScene(activeScene))
            {
                BeginSession(activeScene);
            }
            else if (activeScene.name == SceneLoader.MAIN_SCENE_NAME)
            {
                BeginMainMenu();
            }
        }

        /// <summary>
        /// Main 씬 진입 시 씬의 View로 Presenter를 만든다. View가 없으면 그 UI 없이 진행한다.
        /// </summary>
        private void BeginMainMenu()
        {
            SettingsView settingsView = FindAnyObjectByType<SettingsView>();
            SettingsPresenter settingsPresenter = settingsView != null ? new SettingsPresenter(settingsView) : null;
            LoadingView loadingView = FindAnyObjectByType<LoadingView>();
            _loadingPresenter = loadingView != null ? new LoadingPresenter(loadingView) : null;
            MainMenuView mainMenuView = FindAnyObjectByType<MainMenuView>();
            _mainMenuPresenter = mainMenuView != null ? new MainMenuPresenter(mainMenuView, settingsPresenter, _loadingPresenter) : null;
        }

        /// <summary>
        /// 게임 단계가 Defeat가 된 것을 감지하면 점수와 도달 웨이브를 기존 기록과 비교해 갱신한다 (DESIGN 3.4.2). 판마다 한 번.
        /// 결과 패널보다 먼저 호출한다.
        /// </summary>
        private void SaveRecordOnDefeat()
        {
            if (_hasSavedRecord || _world.flow.phase != GamePhase.Defeat || App.Records == null)
            {
                return;
            }

            _hasSavedRecord = true;
            App.Records.TryUpdate(_world.progress.score, GameFlowSystem.GetCumulativeWaveNumber(_world));
        }

        private static bool IsGameplayScene(Scene scene)
        {
            return scene.name == SceneLoader.GAME_SCENE_NAME || scene.path.StartsWith(TEST_SCENE_FOLDER, global::System.StringComparison.Ordinal);
        }

        private void BeginSession(Scene scene)
        {
            ClearViewPool();
            _world = new World();
            // 씬 배치 데이터를 먼저 채운다. Initialize가 공터 수로 타워 저장소를 만들고 히어로를 시작 위치에 세운다
            SceneBaker.Bake(_world, scene);
            SessionSystem.Initialize(_world, App.Data);
            // 판마다 다른 시드. System은 시간을 읽지 않으므로 Core가 정해서 넘긴다
            SessionSystem.InitializeRandom(_world, global::System.Environment.TickCount);
            _viewContext = new ViewContext();
            // 씬의 표현용 참조 수집 (판 시작 시 1회)
            _viewContext.heroView = FindAnyObjectByType<HeroView>();
            _viewContext.heroCameraView = FindAnyObjectByType<HeroCameraView>();

            // UI Presenter (판 시작 시 1회). View가 씬에 없으면 그 UI 없이 진행한다
            ResultView resultView = FindAnyObjectByType<ResultView>();
            _resultPresenter = resultView != null ? new ResultPresenter(resultView) : null;
            LevelUpChoiceView levelUpChoiceView = FindAnyObjectByType<LevelUpChoiceView>();
            _levelUpChoicePresenter = levelUpChoiceView != null ? new LevelUpChoicePresenter(levelUpChoiceView, _world) : null;
            BuildMenuView buildMenuView = FindAnyObjectByType<BuildMenuView>();
            _buildMenuPresenter = buildMenuView != null ? new BuildMenuPresenter(buildMenuView, _world) : null;
            CoreMenuView coreMenuView = FindAnyObjectByType<CoreMenuView>();
            _coreMenuPresenter = coreMenuView != null ? new CoreMenuPresenter(coreMenuView, _world) : null;
            PauseMenuView pauseMenuView = FindAnyObjectByType<PauseMenuView>();
            _pauseMenuPresenter = pauseMenuView != null ? new PauseMenuPresenter(pauseMenuView, _world) : null;
        }

        private void EndSession()
        {
            // 풀링된 View는 떠난 씬과 함께 파괴됐으므로 참조만 남지 않게 비운다 (DESIGN 5.8)
            if (_world != null)
            {
                ClearViewPool();
            }
            _world = null;
            _viewContext = null;
            _resultPresenter = null;
            _levelUpChoicePresenter = null;
            _buildMenuPresenter = null;
            _coreMenuPresenter = null;
            _pauseMenuPresenter = null;
            _hasSavedRecord = false;
            _mainMenuPresenter = null;
            _loadingPresenter = null;
        }

        private static void ClearViewPool()
        {
            if (App.Pool != null)
            {
                App.Pool.Clear();
            }
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(DEBUG_PANEL_MARGIN, DEBUG_PANEL_MARGIN, DEBUG_PANEL_WIDTH, DEBUG_PANEL_HEIGHT), GUI.skin.box);
            GUILayout.Label("[Runner Debug]");
            GUILayout.Label("Scene: " + SceneManager.GetActiveScene().name);
            GUILayout.Label("Fixed Timestep: " + Time.fixedDeltaTime + " / Max Allowed: " + Time.maximumDeltaTime);
            if (_world == null)
            {
                GUILayout.Label("World: 없음");
            }
            else
            {
                GUILayout.Label("Tick: " + _world.tickCount);
                GUILayout.Label("Phase: " + _world.flow.phase + " (" + _world.flow.stageNumber + "-" + _world.flow.waveNumber + ")");
                GUILayout.Label("Pause: " + _world.pauseReason);
                GUILayout.Label("Hero: " + _world.hero.state + " " + _world.hero.currentPosition + " Path " + _world.hero.nextPathCornerIndex + "/" + _world.hero.pathCornerCount);
                DrawProgressDebug();
                DrawSiegeDebug();
                DrawWaveDebug();
                DrawBakingDebug();
            }
            GUILayout.EndArea();
        }

        /// <summary>
        /// 골드·XP·레벨·레벨업 대기·드롭 수를 보여준다 (feat/drop-progress 확인용. HUD가 생기기 전까지).
        /// </summary>
        private void DrawProgressDebug()
        {
            ProgressState progress = _world.progress;
            int[] requiredXPByLevel = _world.progression.requiredXPByLevel;
            int requiredXP = requiredXPByLevel.Length == 0 ? 0 : requiredXPByLevel[Mathf.Clamp(progress.level - 1, 0, requiredXPByLevel.Length - 1)];
            GUILayout.Label("── Progress ──");
            GUILayout.Label("Gold " + progress.gold + " (" + progress.goldGainPercent + "%) / Lv " + progress.level + " XP " + progress.currentXP + "/" + requiredXP + " (" + progress.xpGainPercent + "%) / Pending " + progress.pendingLevelUpCount);
            bool[] isUnlockedByTowerType = _world.towerUnlock.isUnlockedByTowerType;
            int unlockedCount = 0;
            for (int towerTypeIndex = 0; towerTypeIndex < isUnlockedByTowerType.Length; towerTypeIndex++)
            {
                if (isUnlockedByTowerType[towerTypeIndex])
                {
                    unlockedCount++;
                }
            }
            GUILayout.Label("Tower types unlocked " + unlockedCount + "/" + isUnlockedByTowerType.Length);
            GUILayout.Label("Towers " + _world.towers.Count + "/" + _world.towers.Capacity + " / Build menu slot " + _world.buildMenuSlotIndex + " / Pointer held " + _world.input.isPointerHeld);
            for (int towerIndex = 0; towerIndex < _world.towers.Capacity; towerIndex++)
            {
                if (_world.towers.IsAliveAt(towerIndex))
                {
                    Tower tower = _world.towers.GetAt(towerIndex);
                    GUILayout.Label("  Tower slot " + tower.slotIndex + " Lv " + tower.level + " HP " + tower.currentHP + "/" + tower.maxHP + (tower.isBoarded ? " (Boarded)" : "") + " cooldown " + tower.attackCooldownTicks + "t");
                }
            }
            GUILayout.Label("Drops " + _world.drops.Count + "/" + _world.drops.Capacity + " Queue " + _world.dropSpawnRequests.Count + " / Hero Atk " + _world.hero.attackDamage);
        }

        /// <summary>
        /// 코어 HP·포위 수와 히어로 HP·목숨·무적·부활 상태를 보여준다 (feat/core-siege 확인용).
        /// </summary>
        private void DrawSiegeDebug()
        {
            Hero hero = _world.hero;
            int occupiedSlotCount = 0;
            bool[] isSiegeSlotOccupied = _world.core.isSiegeSlotOccupied;
            for (int slotIndex = 0; slotIndex < isSiegeSlotOccupied.Length; slotIndex++)
            {
                if (isSiegeSlotOccupied[slotIndex])
                {
                    occupiedSlotCount++;
                }
            }

            GUILayout.Label("── Siege ──");
            GUILayout.Label("Core HP " + _world.core.currentHP + "/" + _world.core.maxHP + " / Siege slots used " + occupiedSlotCount);
            GUILayout.Label("Hero HP " + hero.currentHP + "/" + hero.maxHP + " Lives " + hero.remainingLives + " Invincible " + hero.invincibilityRemainingTicks + "t Respawn " + hero.respawnRemainingTicks + "t");
        }

        /// <summary>
        /// 웨이브 진행과 적 수를 보여주고, 처치 판정이 생기기 전까지 웨이브를 넘기는 디버그 버튼을 둔다 (feat/enemy-wave 확인용).
        /// 버튼은 World를 직접 바꾸지 않고 입력 버퍼에 명령만 넣는다 (UI 이벤트 예외 규칙).
        /// </summary>
        private void DrawWaveDebug()
        {
            GameFlowState flow = _world.flow;
            GUILayout.Label("── Wave ──");
            GUILayout.Label("Score " + _world.progress.score + " / Boss " + (flow.bossID.IsValid ? (_world.enemies.IsAlive(flow.bossID) ? "alive" : "dead") : "-"));
            GUILayout.Label("Spawned " + flow.spawnedEnemyCount + "/" + flow.waveEnemyCount + " Kill " + flow.killCount + " Next spawn " + flow.spawnCooldownTicks + "t Prep " + flow.phaseRemainingTicks + "t");
            GUILayout.Label("Enemies alive " + _world.enemies.Count + "/" + _world.enemies.Capacity + " Queue " + _world.enemySpawnRequests.Count);
            GUILayout.Label("Hero attack cooldown " + _world.hero.attackCooldownTicks + "t / Projectiles " + _world.projectiles.Count + "/" + _world.projectiles.Capacity);
            if (GUILayout.Button("[Debug] 웨이브 클리어"))
            {
                Command clearWaveCommand = new Command();
                clearWaveCommand.kind = CommandKind.DebugClearWave;
                _world.input.commands.Enqueue(clearWaveCommand);
            }
        }

        /// <summary>
        /// 베이킹된 값이 World에 들어갔는지 보여준다 (feat/baking 확인용).
        /// </summary>
        private void DrawBakingDebug()
        {
            GUILayout.Label("── Baked: Scene ──");
            GUILayout.Label("Paths: " + _world.waypointPaths.Length + " / Spawns: " + _world.spawnPoints.Length + " / Slots: " + _world.towerSlots.Length + " (Tower cap " + _world.towers.Capacity + ")");
            for (int pathIndex = 0; pathIndex < _world.waypointPaths.Length; pathIndex++)
            {
                GUILayout.Label("  Path " + pathIndex + ": " + _world.waypointPaths[pathIndex].waypoints.Length + " waypoints");
            }
            for (int spawnIndex = 0; spawnIndex < _world.spawnPoints.Length; spawnIndex++)
            {
                GUILayout.Label("  Spawn " + spawnIndex + ": " + _world.spawnPoints[spawnIndex].position + " → Path " + _world.spawnPoints[spawnIndex].pathIndex);
            }
            GUILayout.Label("Core: " + _world.core.position + " HP " + _world.core.currentHP + "/" + _world.core.maxHP);
            GUILayout.Label("Hero: " + _world.hero.currentPosition + " HP " + _world.hero.currentHP + "/" + _world.hero.maxHP + " Lives " + _world.hero.remainingLives);
            GUILayout.Label("  Speed " + _world.hero.moveSpeed + " Atk " + _world.hero.attackDamage + "/" + _world.hero.attackIntervalTicks + "t Melee " + _world.hero.meleeRange + " Ranged " + _world.hero.rangedRange);

            GUILayout.Label("── Baked: Definition ──");
            GUILayout.Label("Enemy types: " + _world.enemyStats.Length);
            for (int enemyTypeIndex = 0; enemyTypeIndex < _world.enemyStats.Length; enemyTypeIndex++)
            {
                ref EnemyStats enemyStats = ref _world.enemyStats[enemyTypeIndex];
                GUILayout.Label("  [" + enemyTypeIndex + "] HP " + enemyStats.baseHP + " Speed " + enemyStats.moveSpeed + " Gold " + enemyStats.baseDropGold + " XP " + enemyStats.baseDropXP + (enemyStats.isBoss ? " (Boss)" : ""));
            }
            GUILayout.Label("Wave: x × " + _world.waveRule.enemyCountPerWaveNumber + ", spawn " + _world.waveRule.spawnIntervalTicks + "t, prep " + _world.waveRule.preparationTicks + "t, boss " + _world.waveRule.bossWaveNumber);
            GUILayout.Label("Difficulty: HP ×" + _world.difficulty.hpGrowth + "^(w-1), Gold +" + _world.difficulty.goldGrowth + ", XP +" + _world.difficulty.xpGrowth);
            GUILayout.Label("Progression: " + _world.progression.requiredXPByLevel.Length + " levels");
        }
#endif
    }
}
