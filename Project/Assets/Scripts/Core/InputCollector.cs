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
        // 레벨업 선택지 1 / 2 / 3 키 (인덱스 = 제시 칸 번호)
        private readonly InputAction[] _chooseOptionActions = new InputAction[ProgressState.OFFERED_OPTION_COUNT];
        private bool _hasSearchedActions;

        private const string CLICK_ACTION_PATH = "Game/Click";
        private const string POINT_ACTION_PATH = "Game/Point";
        private const string INTERACT_ACTION_PATH = "Game/Interact";
        // 레벨업 선택지 키 액션 경로 앞부분. 뒤에 1부터 붙는다 (Game/ChooseOption1 ~ 3)
        private const string CHOOSE_OPTION_ACTION_PATH_PREFIX = "Game/ChooseOption";
        // 클릭 레이캐스트 최대 거리 (카메라에서 지면까지 충분히 먼 값)
        private const float MAX_CLICK_RAY_DISTANCE = 1000f;

        /// <summary>
        /// 이번 프레임에 눌린 입력을 명령으로 바꿔 입력 버퍼에 넣는다.
        /// 좌클릭은 누르고 있는 상태·커서 지면 좌표를 갱신하고(연속 입력), 누른 순간 지상이면 MoveTo. Space는 Interact. 레벨업 선택 중 1 / 2 / 3은 ChooseLevelUpOption.
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

            CollectLevelUpChoice(world);

            CollectPointer(world);
        }

        /// <summary>
        /// 좌클릭. 누르고 있는 상태와 커서 지면 좌표를 매 프레임 입력 버퍼에 갱신한다 (연속 입력).
        /// 누른 순간에는 지상이면 MoveTo 명령도 넣어 바로 반응하게 한다. 탑승 중 사격은 TowerSystem이 누르고 있는 상태를 읽는다.
        /// </summary>
        private void CollectPointer(World world)
        {
            InputBuffer input = world.input;
            input.isPointerHeld = _clickAction.IsPressed();
            if (!input.isPointerHeld)
            {
                input.hasHeldGroundPosition = false;
                return;
            }

            Vector3 groundPosition;
            input.hasHeldGroundPosition = TryRaycastGround(out groundPosition);
            if (!input.hasHeldGroundPosition)
            {
                return;
            }
            input.heldGroundPosition = groundPosition;

            if (_clickAction.WasPressedThisFrame() && world.hero.state != HeroState.Boarded)
            {
                Command moveCommand = new Command();
                moveCommand.kind = CommandKind.MoveTo;
                moveCommand.groundPosition = groundPosition;
                input.commands.Enqueue(moveCommand);
            }
        }

        /// <summary>
        /// 레벨업 선택 중에 1 / 2 / 3 키가 눌리면 그 칸을 고르는 명령을 넣는다 (DESIGN 2장, 임시 키).
        /// 선택 중이 아니면 읽지 않는다. 같은 프레임에 여러 키가 눌려도 CommandSystem이 한 틱에 하나만 받는다.
        /// </summary>
        private void CollectLevelUpChoice(World world)
        {
            if (world.pauseReason != PauseReason.LevelUpChoice)
            {
                return;
            }

            for (int offeredSlot = 0; offeredSlot < _chooseOptionActions.Length; offeredSlot++)
            {
                InputAction chooseAction = _chooseOptionActions[offeredSlot];
                if (chooseAction == null || !chooseAction.WasPressedThisFrame())
                {
                    continue;
                }

                Command chooseCommand = new Command();
                chooseCommand.kind = CommandKind.ChooseLevelUpOption;
                chooseCommand.optionIndex = offeredSlot;
                world.input.commands.Enqueue(chooseCommand);
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
                // 선택지 키는 없어도 클릭으로 고를 수 있으므로 필수가 아니다 (액션 찾기는 1회라 문자열 결합은 문제없다)
                for (int offeredSlot = 0; offeredSlot < _chooseOptionActions.Length; offeredSlot++)
                {
                    _chooseOptionActions[offeredSlot] = actions.FindAction(CHOOSE_OPTION_ACTION_PATH_PREFIX + (offeredSlot + 1));
                }
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
