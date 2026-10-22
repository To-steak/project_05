using UnityEngine;
using UnityEngine.UI;

namespace Project05.UI.View
{
    /// <summary>
    /// [Game] 일시정지 메뉴 (ESC). 계속, 메인으로 버튼. 표시만 담당하며 World와 App을 모른다.
    /// 이 컴포넌트는 항상 켜진 오브젝트에 두고, 실제 패널은 _panelRoot로 켜고 끈다.
    /// </summary>
    public class PauseMenuView : MonoBehaviour
    {
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _mainMenuButton;

        /// <summary>
        /// 계속 버튼. Presenter가 onClick을 구독한다.
        /// </summary>
        public Button ResumeButton
        {
            get { return _resumeButton; }
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
    }
}
