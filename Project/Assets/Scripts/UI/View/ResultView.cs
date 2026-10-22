using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project05.UI.View
{
    /// <summary>
    /// [Game] 결과 패널. 점수·도달 웨이브 텍스트와 재시작·메인으로 버튼. 표시만 담당하며 World와 App을 모른다.
    /// 이 컴포넌트는 항상 켜진 오브젝트(예: Canvas)에 두고, 실제 패널은 _panelRoot로 켜고 끈다 (꺼진 오브젝트는 Runner가 찾지 못한다).
    /// </summary>
    public class ResultView : MonoBehaviour
    {
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private TMP_Text _resultText;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _mainMenuButton;

        /// <summary>
        /// 재시작 버튼. Presenter가 onClick을 구독한다.
        /// </summary>
        public Button RestartButton
        {
            get { return _restartButton; }
        }

        /// <summary>
        /// 메인으로 버튼. Presenter가 onClick을 구독한다.
        /// </summary>
        public Button MainMenuButton
        {
            get { return _mainMenuButton; }
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
        /// 결과 텍스트를 바꾼다.
        /// </summary>
        /// <param name="text">표시할 문장 (Presenter가 만든다)</param>
        public void SetResultText(string text)
        {
            if (_resultText != null)
            {
                _resultText.text = text;
            }
        }
    }
}
