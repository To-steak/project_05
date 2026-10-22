using Project05.Component;
using Project05.Component.Data;
using Project05.Component.Input;
using Project05.Foundation;
using Project05.UI.View;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Project05.UI.Presenter
{
    /// <summary>
    /// [Game] 일시정지 사유가 LevelUpChoice일 때 선택지 패널을 띄운다 (DESIGN 3.6). Runner가 LateUpdate에서 매 프레임 호출한다.
    /// 버튼은 UI 이벤트 예외(onClick)로 받고, World를 바꾸지 않고 ChooseLevelUpOption 명령만 넣는다.
    /// 제시된 선택지가 바뀐 프레임에만 문구를 다시 만든다 (매 프레임 문자열 할당을 피함).
    /// </summary>
    public class LevelUpChoicePresenter
    {
        private readonly LevelUpChoiceView _view;
        private readonly World _world;
        // 지금 화면에 보이는 선택지 (바뀌었는지 비교용)
        private readonly int[] _shownOptionIndices = new int[ProgressState.OFFERED_OPTION_COUNT];
        private bool _isShown;

        // 퍼센트 포인트로 더하는 선택지에 붙이는 단위
        private const string PERCENT_SUFFIX = "%p";

        /// <summary>
        /// 선택지 패널 View와 이번 판의 월드를 받아 버튼을 연결하고 패널을 숨긴다.
        /// </summary>
        /// <param name="view">씬의 선택지 패널 View</param>
        /// <param name="world">이번 판의 월드 (버튼 명령을 넣을 곳)</param>
        public LevelUpChoicePresenter(LevelUpChoiceView view, World world)
        {
            _view = view;
            _world = world;
            _view.SetVisible(false);

            for (int offeredSlot = 0; offeredSlot < _view.OptionCount; offeredSlot++)
            {
                Button optionButton = _view.GetOptionButton(offeredSlot);
                if (optionButton != null)
                {
                    optionButton.onClick.AddListener(CreateChooseHandler(offeredSlot));
                }
            }
        }

        /// <summary>
        /// 레벨업 선택 중이면 패널을 띄우고, 제시된 선택지가 바뀌었으면 문구를 갱신한다. 아니면 숨긴다.
        /// </summary>
        /// <param name="world">현재 판의 월드 (읽기 전용)</param>
        public void Present(World world)
        {
            bool isChoosing = world.pauseReason == PauseReason.LevelUpChoice;
            if (!isChoosing)
            {
                if (_isShown)
                {
                    _isShown = false;
                    _view.SetVisible(false);
                }
                return;
            }

            int[] offeredOptionIndices = world.progress.offeredOptionIndices;
            if (_isShown && IsSameAsShown(offeredOptionIndices))
            {
                return;
            }

            for (int offeredSlot = 0; offeredSlot < _view.OptionCount; offeredSlot++)
            {
                int optionIndex = offeredSlot < offeredOptionIndices.Length ? offeredOptionIndices[offeredSlot] : ProgressState.NO_OFFERED_OPTION;
                bool isAvailable = optionIndex != ProgressState.NO_OFFERED_OPTION;
                _view.SetOption(offeredSlot, isAvailable ? BuildOptionText(world, offeredSlot, optionIndex) : string.Empty, isAvailable);
            }

            for (int offeredSlot = 0; offeredSlot < _shownOptionIndices.Length; offeredSlot++)
            {
                _shownOptionIndices[offeredSlot] = offeredSlot < offeredOptionIndices.Length ? offeredOptionIndices[offeredSlot] : ProgressState.NO_OFFERED_OPTION;
            }
            _isShown = true;
            _view.SetVisible(true);
        }

        private bool IsSameAsShown(int[] offeredOptionIndices)
        {
            for (int offeredSlot = 0; offeredSlot < _shownOptionIndices.Length && offeredSlot < offeredOptionIndices.Length; offeredSlot++)
            {
                if (_shownOptionIndices[offeredSlot] != offeredOptionIndices[offeredSlot])
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// "1. 캐릭터 HP +20", "4. 골드 획득량 +10%p" 형식으로 만든다. 이름은 ViewData, 증가량은 World의 테이블에서 읽는다.
        /// </summary>
        private static string BuildOptionText(World world, int offeredSlot, int optionIndex)
        {
            string[] optionNames = App.ViewData.levelUpOptionNames;
            string optionName = optionIndex < optionNames.Length ? optionNames[optionIndex] : string.Empty;
            ref LevelUpOptionStats option = ref world.levelUpOptions[optionIndex];
            bool isPercent = option.effectKind == LevelUpEffectKind.GoldGainPercent || option.effectKind == LevelUpEffectKind.XPGainPercent;
            return (offeredSlot + 1) + ". " + optionName + " +" + option.amount + (isPercent ? PERCENT_SUFFIX : string.Empty);
        }

        /// <summary>
        /// 버튼마다 자기 칸 번호를 담은 핸들러를 만든다 (판 시작 시 1회).
        /// </summary>
        private UnityAction CreateChooseHandler(int offeredSlot)
        {
            return () => EnqueueChoose(offeredSlot);
        }

        private void EnqueueChoose(int offeredSlot)
        {
            Command chooseCommand = new Command();
            chooseCommand.kind = CommandKind.ChooseLevelUpOption;
            chooseCommand.optionIndex = offeredSlot;
            _world.input.commands.Enqueue(chooseCommand);
        }
    }
}
