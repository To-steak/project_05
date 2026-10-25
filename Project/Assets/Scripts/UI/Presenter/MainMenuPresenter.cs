using Project05.Foundation;
using Project05.UI.View;

namespace Project05.UI.Presenter
{
    /// <summary>
    /// [Main] 메인 메뉴. 최고 점수·최고 도달 웨이브(App.Records)를 표시하고, 시작 버튼은 로딩 패널을, 설정 버튼은 설정 패널을 연다.
    /// 로딩 패널이 씬에 없으면 시작 버튼이 Game 씬을 바로(동기) 로드한다.
    /// </summary>
    public class MainMenuPresenter
    {
        private readonly MainMenuView _view;
        private readonly SettingsPresenter _settingsPresenter;
        private readonly LoadingPresenter _loadingPresenter;

        private const string NO_RECORD_TEXT = "-";

        /// <summary>
        /// 메인 메뉴 View와 함께 열 패널의 Presenter를 받아 버튼을 연결하고 기록을 표시한다.
        /// </summary>
        /// <param name="view">씬의 메인 메뉴 View</param>
        /// <param name="settingsPresenter">설정 패널 Presenter (없으면 null)</param>
        /// <param name="loadingPresenter">로딩 패널 Presenter (없으면 null)</param>
        public MainMenuPresenter(MainMenuView view, SettingsPresenter settingsPresenter, LoadingPresenter loadingPresenter)
        {
            _view = view;
            _settingsPresenter = settingsPresenter;
            _loadingPresenter = loadingPresenter;

            if (_view.StartButton != null)
            {
                _view.StartButton.onClick.AddListener(OnStartClicked);
            }
            if (_view.SettingsButton != null)
            {
                _view.SettingsButton.onClick.AddListener(OnSettingsClicked);
            }

            // Main 씬에 들어올 때 한 번 만드는 문자열이라 할당은 문제없다
            _view.SetScoreText("최고 점수: " + App.Records.BestScore);
            _view.SetWaveText("최고 기록: " + FormatWave(App.Records.BestCumulativeWaveNumber));
        }

        /// <summary>
        /// 누적 웨이브 번호 w를 n-x로 바꾼다 (DESIGN 3.4.2). 스테이지당 웨이브 수는 웨이브 규칙의 보스 웨이브 번호.
        /// </summary>
        private static string FormatWave(int cumulativeWaveNumber)
        {
            int wavesPerStage = App.Data.waveRule.bossWaveNumber;
            if (cumulativeWaveNumber <= 0 || wavesPerStage <= 0)
            {
                return NO_RECORD_TEXT;
            }

            int stageNumber = (cumulativeWaveNumber - 1) / wavesPerStage + 1;
            int waveNumber = (cumulativeWaveNumber - 1) % wavesPerStage + 1;
            return stageNumber + "-" + waveNumber;
        }

        private void OnStartClicked()
        {
            if (_loadingPresenter != null)
            {
                _loadingPresenter.Begin();
                return;
            }
            App.Scene.LoadGame();
        }

        private void OnSettingsClicked()
        {
            if (_settingsPresenter != null)
            {
                _settingsPresenter.Open();
            }
        }
    }
}
