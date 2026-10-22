using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project05.UI.View
{
    /// <summary>
    /// [Main] 로딩 패널. 진행률(슬라이더·텍스트)과 로드가 끝나면 누를 수 있는 시작 버튼. 표시만 담당하며 World와 App을 모른다.
    /// 이 컴포넌트는 항상 켜진 오브젝트에 두고, 실제 패널은 _panelRoot로 켜고 끈다.
    /// </summary>
    public class LoadingView : MonoBehaviour
    {
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private Slider _progressSlider;
        [SerializeField] private TMP_Text _progressText;
        [SerializeField] private Button _startButton;

        /// <summary>
        /// 시작 버튼. Presenter가 onClick을 구독한다.
        /// </summary>
        public Button StartButton
        {
            get { return _startButton; }
        }

        /// <summary>
        /// 패널을 보이거나 숨긴다.
        /// </summary>
        /// <param name="isVisible">보이면 true</param>
        public void SetVisible(bool isVisible)
        {
            if (_panelRoot != null && _panelRoot.activeSelf != isVisible)
            {
                _panelRoot.SetActive(isVisible);
            }
        }

        /// <summary>
        /// 진행률을 표시한다.
        /// </summary>
        /// <param name="progress">진행률 (0 ~ 1)</param>
        /// <param name="text">진행률 문구 (Presenter가 만든다, 바뀔 때만 넘긴다. null이면 그대로)</param>
        public void SetProgress(float progress, string text)
        {
            if (_progressSlider != null)
            {
                _progressSlider.SetValueWithoutNotify(progress);
            }
            if (_progressText != null && text != null)
            {
                _progressText.text = text;
            }
        }

        /// <summary>
        /// 시작 버튼을 누를 수 있게 하거나 막는다.
        /// </summary>
        /// <param name="isInteractable">누를 수 있으면 true</param>
        public void SetStartInteractable(bool isInteractable)
        {
            if (_startButton != null && _startButton.interactable != isInteractable)
            {
                _startButton.interactable = isInteractable;
            }
        }
    }
}
