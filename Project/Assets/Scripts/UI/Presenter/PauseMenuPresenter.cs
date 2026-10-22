using Project05.Component;
using Project05.Component.Input;
using Project05.Foundation;
using Project05.UI.View;

namespace Project05.UI.Presenter
{
    /// <summary>
    /// [Game] 일시정지 사유가 PauseMenu일 때(ESC) 일시정지 메뉴를 띄운다. Runner가 LateUpdate에서 매 프레임 호출한다.
    /// 계속 버튼은 ClosePauseMenu 명령을 넣고, 메인으로 버튼은 App.Scene으로 Main 씬을 로드한다 (판 중간에 나가면 기록은 저장하지 않는다).
    /// </summary>
    public class PauseMenuPresenter
    {
        private readonly PauseMenuView _view;
        private readonly World _world;
        private bool _isShown;

        /// <summary>
        /// 일시정지 메뉴 View와 이번 판의 월드를 받아 버튼을 연결하고 패널을 숨긴다.
        /// </summary>
        /// <param name="view">씬의 일시정지 메뉴 View</param>
        /// <param name="world">이번 판의 월드 (계속 버튼 명령을 넣을 곳)</param>
        public PauseMenuPresenter(PauseMenuView view, World world)
        {
            _view = view;
            _world = world;
            _view.SetVisible(false);
            if (_view.ResumeButton != null)
            {
                _view.ResumeButton.onClick.AddListener(OnResumeClicked);
            }
            if (_view.MainMenuButton != null)
            {
                _view.MainMenuButton.onClick.AddListener(OnMainMenuClicked);
            }
        }

        /// <summary>
        /// 일시정지 메뉴가 열려 있으면 띄우고 아니면 숨긴다.
        /// </summary>
        /// <param name="world">현재 판의 월드 (읽기 전용)</param>
        public void Present(World world)
        {
            bool isOpen = world.pauseReason == PauseReason.PauseMenu;
            if (isOpen != _isShown)
            {
                _isShown = isOpen;
                _view.SetVisible(isOpen);
            }
        }

        private void OnResumeClicked()
        {
            Command closeCommand = new Command();
            closeCommand.kind = CommandKind.ClosePauseMenu;
            _world.input.commands.Enqueue(closeCommand);
        }

        private void OnMainMenuClicked()
        {
            App.Scene.LoadMain();
        }
    }
}
