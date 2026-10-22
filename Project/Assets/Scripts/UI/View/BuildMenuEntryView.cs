using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project05.UI.View
{
    /// <summary>
    /// [Game] 건설 목록의 한 칸 (타워 종류 하나). 아이콘 이미지, 건설 버튼, 비용 텍스트(이름 텍스트는 선택). 표시만 담당한다.
    /// BuildMenuView가 템플릿을 복제해 타워 종류 수만큼 만든다.
    /// </summary>
    public class BuildMenuEntryView : MonoBehaviour
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private Button _buildButton;
        [SerializeField] private TMP_Text _costText;
        [SerializeField] private TMP_Text _nameText;

        /// <summary>
        /// 건설 버튼. Presenter가 onClick을 구독한다.
        /// </summary>
        public Button BuildButton
        {
            get { return _buildButton; }
        }

        /// <summary>
        /// 아이콘·이름·비용을 채운다. 판 시작 시 한 번 호출한다.
        /// </summary>
        /// <param name="icon">타워 아이콘 (없으면 이미지를 숨긴다)</param>
        /// <param name="displayName">타워 이름</param>
        /// <param name="costText">비용 문구 (Presenter가 만든다)</param>
        public void SetContent(Sprite icon, string displayName, string costText)
        {
            if (_iconImage != null)
            {
                _iconImage.sprite = icon;
                _iconImage.enabled = icon != null;
            }
            if (_nameText != null)
            {
                _nameText.text = displayName;
            }
            if (_costText != null)
            {
                _costText.text = costText;
            }
        }

        /// <summary>
        /// 칸을 보이거나 숨긴다 (해금되지 않은 종류는 숨긴다).
        /// </summary>
        /// <param name="isVisible">보이면 true</param>
        public void SetVisible(bool isVisible)
        {
            if (gameObject.activeSelf != isVisible)
            {
                gameObject.SetActive(isVisible);
            }
        }

        /// <summary>
        /// 건설 버튼을 누를 수 있게 하거나 막는다 (골드가 모자라면 막는다).
        /// </summary>
        /// <param name="isInteractable">누를 수 있으면 true</param>
        public void SetBuildInteractable(bool isInteractable)
        {
            if (_buildButton != null && _buildButton.interactable != isInteractable)
            {
                _buildButton.interactable = isInteractable;
            }
        }
    }
}
