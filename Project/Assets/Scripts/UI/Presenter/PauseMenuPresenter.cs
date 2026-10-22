using Project05.Component;
using Project05.Component.Input;
using Project05.Foundation;
using Project05.UI.View;

namespace Project05.UI.Presenter
{
    /// <summary>
    /// [Game] 일시정지 사유가 PauseMenu일 때(ESC) 일시정지 메뉴를 띄운다. Runner가 LateUpdate에서 매 프레임 호출한다.
    /// 계속 버튼은 ClosePauseMenu 명령을 넣고, 설정 버튼은 설정 패널을 열고, 메인으로 버튼은 App.Scene으로 Main 씬을 로드한다 (판 중간에 나가면 기록은 저장하지 않는다).
    /// </summary>
    public class PauseMenuPresenter
    {
        private readonly PauseMenuView _view;
        private readonly World _world;
        private readonly SettingsPresenter _settingsPresenter;
        private bool _isShown;

        /// <summary>
        /// 일시정지 메뉴 View와 이번 판의 월드를 받아 버튼을 연결하고 패널을 숨긴다.
        /// </summary>
        /// <param name="view">씬의 일시정지 메뉴 View</param>
        /// <param name="world">이번 판의 월드 (계속 버튼 명령을 넣을 곳)</param>
        /// <param name="settingsPresenter">설정 패널 Presenter (씬에 없으면 null, 그때 설정 버튼은 아무것도 하지 않는다)</param>
        public PauseMenuPresenter(PauseMenuView view, World world, SettingsPresenter settingsPresenter)
        {
            _view = view;
            _world = world;
            _settingsPresenter = settingsPresenter;
            _view.SetVisible(false);
            if (_view.ResumeButton != null)
            {
                _view.ResumeButton.onClick.AddListener(OnResumeClicked);
            }
            if (_view.SettingsButton != null)
            {
                _view.SettingsButton.onClick.AddListener(OnSettingsClicked);
            }
            if (_view.MainMenuButton != null)
            {
                _view.MainMenuButton.onClick.AddListener(OnMainMenuClicked);
            }
        }

        /// <summary>
        /// 일시정지 메뉴가 열려 있으면 띄우고 아니면 숨긴다. 메뉴가 닫히면(계속, ESC) 열려 있던 설정 패널도 저장하고 닫는다.
        /// </summary>
        /// <param name="world">현재 판의 월드 (읽기 전용)</param>
        public void Present(World world)
        {
            bool isOpen = world.pauseReason == PauseReason.PauseMenu;
            if (isOpen == _isShown)
            {
                return;
            }

            _isShown = isOpen;
            _view.SetVisible(isOpen);
            if (!isOpen && _settingsPresenter != null)
            {
                _settingsPresenter.Close();
            }
        }

        private void OnSettingsClicked()
        {
            if (_settingsPresenter != null)
            {
                _settingsPresenter.Open();
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
