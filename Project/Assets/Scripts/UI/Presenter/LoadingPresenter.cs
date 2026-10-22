using Project05.Foundation;
using Project05.UI.View;
using UnityEngine;

namespace Project05.UI.Presenter
{
    /// <summary>
    /// [Main] 로딩 패널 (DESIGN 3.7). Begin으로 Game 씬 비동기 로드를 시작하고 패널을 띄운다.
    /// Runner가 매 프레임 Present를 호출해 진행률을 갱신하며, 로드가 끝나면(0.9) 시작 버튼을 켠다.
    /// 시작 버튼을 누르면 씬을 활성화한다. 이 클릭이 WebGL 오디오 잠금 해제용 사용자 입력도 겸한다.
    /// </summary>
    public class LoadingPresenter
    {
        private readonly LoadingView _view;
        private bool _isShown;
        // 마지막으로 표시한 퍼센트 (바뀔 때만 문구를 다시 만든다)
        private int _shownPercent = -1;

        private const float PERCENT_SCALE = 100f;

        /// <summary>
        /// 로딩 패널 View를 받아 시작 버튼을 연결하고 패널을 숨긴다.
        /// </summary>
        /// <param name="view">씬의 로딩 패널 View</param>
        public LoadingPresenter(LoadingView view)
        {
            _view = view;
            _view.SetVisible(false);
            if (_view.StartButton != null)
            {
                _view.StartButton.onClick.AddListener(OnStartClicked);
            }
        }

        /// <summary>
        /// Game 씬 비동기 로드를 시작하고 패널을 띄운다. 시작 버튼은 로드가 끝날 때까지 막는다.
        /// </summary>
        public void Begin()
        {
            App.Scene.BeginLoadGame();
            _view.SetStartInteractable(false);
            _view.SetVisible(true);
            _isShown = true;
        }

        /// <summary>
        /// 로드 중이면 진행률과 시작 버튼 상태를 갱신한다.
        /// </summary>
        public void Present()
        {
            if (!_isShown || !App.Scene.IsLoading)
            {
                return;
            }

            float progress = App.Scene.LoadProgress;
            int percent = Mathf.RoundToInt(progress * PERCENT_SCALE);
            string text = null;
            if (percent != _shownPercent)
            {
                _shownPercent = percent;
                text = percent + "%";
            }
            _view.SetProgress(progress, text);
            _view.SetStartInteractable(App.Scene.IsLoadReady);
        }

        private void OnStartClicked()
        {
            App.Scene.ActivateLoadedGame();
        }
    }
}
