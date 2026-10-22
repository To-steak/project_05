using Project05.Component;
using Project05.System;
using Project05.View;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project05.Core
{
    /// <summary>
    /// DDOL에 올라가는 유일한 오브젝트. 현재 판의 World와 ViewContext를 보관하고 루프를 돌린다.
    /// FixedUpdate: System Tick / Update: 입력 수집 / LateUpdate: ViewSystem → Presenter → 프레임 기록 비우기.
    /// 실행 순서는 기본값(0)으로 두어 CinemachineBrain(100)보다 먼저 LateUpdate를 돈다.
    /// </summary>
    public class Runner : MonoBehaviour
    {
        private World _world;
        private ViewContext _viewContext;
        private int _activeSceneHandle;
        private bool _hasActiveScene;

        private const string GAME_SCENE_NAME = "Game";

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

            // [Tick 2 ~ 9] DESIGN 5.5 순서. SpawnSystem은 반드시 마지막
            GameFlowSystem.Tick(_world);
            WaveSystem.Tick(_world);
            HeroSystem.Tick(_world);
            TowerSystem.Tick(_world);
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

            // 입력 수집 → world.input (InputCollector, feat/hero-movement)
        }

        private void LateUpdate()
        {
            SynchronizeSession();
            if (_world == null)
            {
                return;
            }

            // ① ViewSystem: 엔티티 동기화·보간 → 연출. 카메라는 히어로 보간 이후
            HeroViewSystem.Present(_world, _viewContext);
            TowerViewSystem.Present(_world, _viewContext);
            EnemyViewSystem.Present(_world, _viewContext);
            DropViewSystem.Present(_world, _viewContext);
            CoreViewSystem.Present(_world, _viewContext);
            InteractionPromptViewSystem.Present(_world, _viewContext);
            OffscreenIndicatorViewSystem.Present(_world, _viewContext);
            CameraViewSystem.Present(_world, _viewContext);

            // ② Presenter 호출 (등록 방식은 UI 구현 시 결정)

            // ③ 이번 프레임의 기록은 모두 연출됐으므로 비운다
            SessionSystem.ClearFrameRecords(_world);
        }

        /// <summary>
        /// 활성 씬이 바뀌었는지 매 루프 시작 시 확인해 판을 시작하거나 끝낸다.
        /// 이벤트 사용 금지 규칙에 따라 SceneManager 이벤트를 구독하지 않고 활성 씬 handle을 비교한다.
        /// Game 씬 재로드(재시작)도 handle이 바뀌므로 새 판으로 처리된다.
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

            if (activeScene.name == GAME_SCENE_NAME)
            {
                BeginSession();
            }
        }

        private void BeginSession()
        {
            _world = new World();
            SessionSystem.Initialize(_world);
            _viewContext = new ViewContext();
        }

        private void EndSession()
        {
            _world = null;
            _viewContext = null;
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10f, 10f, 320f, 200f), GUI.skin.box);
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
            }
            GUILayout.EndArea();
        }
#endif
    }
}
