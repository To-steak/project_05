using Project05.Component;
using Project05.Component.Entity;
using Project05.Component.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project05.Core
{
    /// <summary>
    /// 프로젝트 전역 입력 액션(InputSystem_Actions의 Game 맵)에서 좌클릭·Space를 읽어 world.input에 Command로 적재한다.
    /// Runner가 Update에서만 호출한다 (wasPressedThisFrame 계열은 Update 전용). World는 읽기만 하고 입력 버퍼에만 쓴다.
    /// </summary>
    public class InputCollector
    {
        private InputAction _clickAction;
        private InputAction _pointAction;
        private InputAction _interactAction;
        private bool _hasSearchedActions;

        private const string CLICK_ACTION_PATH = "Game/Click";
        private const string POINT_ACTION_PATH = "Game/Point";
        private const string INTERACT_ACTION_PATH = "Game/Interact";
        // 클릭 레이캐스트 최대 거리 (카메라에서 지면까지 충분히 먼 값)
        private const float MAX_CLICK_RAY_DISTANCE = 1000f;

        /// <summary>
        /// 이번 프레임에 눌린 입력을 명령으로 바꿔 입력 버퍼에 넣는다.
        /// 좌클릭은 지면 레이캐스트 후 지상이면 MoveTo, 탑승 중이면 Fire. Space는 Interact.
        /// </summary>
        /// <param name="world">현재 판의 월드</param>
        public void Collect(World world)
        {
            if (!TryFindActions())
            {
                return;
            }

            if (_interactAction.WasPressedThisFrame())
            {
                Command interactCommand = new Command();
                interactCommand.kind = CommandKind.Interact;
                world.input.commands.Enqueue(interactCommand);
            }

            if (_clickAction.WasPressedThisFrame())
            {
                Vector3 groundPosition;
                if (TryRaycastGround(out groundPosition))
                {
                    Command clickCommand = new Command();
                    clickCommand.kind = world.hero.state == HeroState.Boarded ? CommandKind.Fire : CommandKind.MoveTo;
                    clickCommand.groundPosition = groundPosition;
                    world.input.commands.Enqueue(clickCommand);
                }
            }
        }

        /// <summary>
        /// 액션을 처음 한 번만 찾아 둔다. 프로젝트 전역 액션은 부트스트랩 시점에 준비되지 않았을 수 있어 첫 사용 때 찾는다.
        /// </summary>
        private bool TryFindActions()
        {
            if (!_hasSearchedActions)
            {
                _hasSearchedActions = true;
                InputActionAsset actions = InputSystem.actions;
                if (actions == null)
                {
                    Debug.LogError("[InputCollector] 프로젝트 전역 입력 액션(InputSystem_Actions)이 설정되지 않았습니다.");
                    return false;
                }

                _clickAction = actions.FindAction(CLICK_ACTION_PATH);
                _pointAction = actions.FindAction(POINT_ACTION_PATH);
                _interactAction = actions.FindAction(INTERACT_ACTION_PATH);
                if (_clickAction == null || _pointAction == null || _interactAction == null)
                {
                    Debug.LogError("[InputCollector] InputSystem_Actions에 Game 맵의 Click / Point / Interact 액션이 없습니다.");
                }
            }

            return _clickAction != null && _pointAction != null && _interactAction != null;
        }

        private bool TryRaycastGround(out Vector3 groundPosition)
        {
            groundPosition = Vector3.zero;

            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                return false;
            }

            Ray ray = mainCamera.ScreenPointToRay(_pointAction.ReadValue<Vector2>());
            RaycastHit hit;
            if (!Physics.Raycast(ray, out hit, MAX_CLICK_RAY_DISTANCE, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                return false;
            }

            groundPosition = hit.point;
            return true;
        }
    }
}
