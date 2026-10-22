using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project05.UI.View
{
    /// <summary>
    /// [Game] 레벨업 선택지 패널. 선택지 버튼 3개와 각 버튼의 설명 텍스트. 표시만 담당하며 World와 App을 모른다.
    /// 이 컴포넌트는 항상 켜진 오브젝트(예: Canvas)에 두고, 실제 패널은 _panelRoot로 켜고 끈다.
    /// 버튼·텍스트 배열의 인덱스가 제시 칸 번호(0 ~ 2)다.
    /// </summary>
    public class LevelUpChoiceView : MonoBehaviour
    {
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private Button[] _optionButtons = new Button[0];
        [SerializeField] private TMP_Text[] _optionTexts = new TMP_Text[0];

        /// <summary>
        /// 선택지 칸 수 (버튼 수)
        /// </summary>
        public int OptionCount
        {
            get { return _optionButtons.Length; }
        }

        /// <summary>
        /// 선택지 버튼. Presenter가 onClick을 구독한다.
        /// </summary>
        /// <param name="offeredSlot">제시 칸 번호</param>
        /// <returns>버튼 (없으면 null)</returns>
        public Button GetOptionButton(int offeredSlot)
        {
            return offeredSlot >= 0 && offeredSlot < _optionButtons.Length ? _optionButtons[offeredSlot] : null;
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
        /// 선택지 한 칸을 채운다. 빈 칸이면 버튼을 숨긴다.
        /// </summary>
        /// <param name="offeredSlot">제시 칸 번호</param>
        /// <param name="text">설명 (Presenter가 만든다)</param>
        /// <param name="isAvailable">고를 수 있는 선택지가 있으면 true</param>
        public void SetOption(int offeredSlot, string text, bool isAvailable)
        {
            if (offeredSlot < _optionTexts.Length && _optionTexts[offeredSlot] != null)
            {
                _optionTexts[offeredSlot].text = text;
            }
            Button optionButton = GetOptionButton(offeredSlot);
            if (optionButton != null && optionButton.gameObject.activeSelf != isAvailable)
            {
                optionButton.gameObject.SetActive(isAvailable);
            }
        }
    }
}
