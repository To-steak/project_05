using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project05.UI.View
{
    /// <summary>
    /// [Game] 코어 UI 해금 탭의 한 칸 (타워 종류 하나). 아이콘, 이름, 해금 비용, 해금 버튼, 해금됨 표시. 표시만 담당한다.
    /// CoreMenuView가 템플릿을 복제해 타워 종류 수만큼 만든다.
    /// </summary>
    public class CoreUnlockEntryView : MonoBehaviour
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _costText;
        [SerializeField] private Button _unlockButton;
        [SerializeField] private GameObject _unlockedMark;

        /// <summary>
        /// 해금 버튼. Presenter가 onClick을 구독한다.
        /// </summary>
        public Button UnlockButton
        {
            get { return _unlockButton; }
        }

        /// <summary>
        /// 아이콘·이름·비용을 채운다. 판 시작 시 한 번 호출한다.
        /// </summary>
        /// <param name="icon">타워 아이콘 (없으면 이미지를 숨긴다)</param>
        /// <param name="displayName">타워 이름</param>
        /// <param name="costText">해금 비용 문구</param>
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
        /// 해금 상태를 표시한다. 해금됐으면 버튼·비용을 숨기고 해금됨 표시를 켠다.
        /// </summary>
        /// <param name="isUnlocked">해금됐으면 true</param>
        /// <param name="canAfford">해금 비용을 낼 수 있으면 true (버튼 활성)</param>
        public void SetState(bool isUnlocked, bool canAfford)
        {
            SetActive(_unlockButton != null ? _unlockButton.gameObject : null, !isUnlocked);
            SetActive(_costText != null ? _costText.gameObject : null, !isUnlocked);
            SetActive(_unlockedMark, isUnlocked);
            if (_unlockButton != null && _unlockButton.interactable != canAfford)
            {
                _unlockButton.interactable = canAfford;
            }
        }

        private static void SetActive(GameObject target, bool isActive)
        {
            if (target != null && target.activeSelf != isActive)
            {
                target.SetActive(isActive);
            }
        }
    }
}
