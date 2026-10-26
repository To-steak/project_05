using Project05.Component;
using Project05.Foundation;
using Project05.UI.View;

namespace Project05.UI.Presenter
{
    /// <summary>
    /// [Game] 게임 단계가 Defeat가 되면 결과 패널을 띄운다 (DESIGN 3.5.2). Runner가 LateUpdate에서 매 프레임 호출한다.
    /// 재시작·메인으로 버튼은 UI 이벤트 예외(onClick)로 받고, World가 아니라 App.Scene을 호출한다.
    /// 한 판 동안 Runner가 보관하며, 판이 끝나면 버린다 (버튼은 씬과 함께 파괴되므로 구독 해제가 필요 없다).
    /// </summary>
    public class ResultPresenter
    {
        private readonly ResultView _view;
        private bool _isShown;

        /// <summary>
        /// 결과 패널 View를 받아 버튼을 연결하고 패널을 숨긴다.
        /// </summary>
        /// <param name="view">씬의 결과 패널 View</param>
        public ResultPresenter(ResultView view)
        {
            _view = view;
            _view.SetVisible(false);
            if (_view.RestartButton != null)
            {
                _view.RestartButton.onClick.AddListener(OnRestartClicked);
            }
            if (_view.MainMenuButton != null)
            {
                _view.MainMenuButton.onClick.AddListener(OnMainMenuClicked);
            }
        }

        /// <summary>
        /// 패배했으면 점수와 도달 웨이브를 채워 패널을 한 번 띄운다. 그 전에는 아무것도 하지 않는다.
        /// </summary>
        /// <param name="world">현재 판의 월드 (읽기 전용)</param>
        public void Present(World world)
        {
            if (_isShown || world.flow.phase != GamePhase.Defeat)
            {
                return;
            }

            _isShown = true;
            // 패배 시 한 번만 만드는 문자열이라 할당은 문제없다
            _view.SetWaveText("도달 웨이브 " + world.flow.stageNumber + "-" + world.flow.waveNumber);
            _view.SetScoreText("점수 " + world.progress.score);
            _view.SetVisible(true);
        }

        private void OnRestartClicked()
        {
            App.Scene.LoadGame();
        }

        private void OnMainMenuClicked()
        {
            App.Scene.LoadMain();
        }
    }
}
