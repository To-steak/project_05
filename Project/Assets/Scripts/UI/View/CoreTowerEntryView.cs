using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project05.UI.View
{
    /// <summary>
    /// [Game] 코어 UI 타워 탭의 한 칸 (공터 하나). 상태 문구(타워 이름·단계·HP 또는 비어 있음), 업그레이드 버튼과 비용, 수리 버튼과 비용.
    /// 표시만 담당한다. CoreMenuView가 템플릿을 복제해 공터 수만큼 만든다.
    /// </summary>
    public class CoreTowerEntryView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _infoText;
        [SerializeField] private Button _upgradeButton;
        [SerializeField] private TMP_Text _upgradeCostText;
        [SerializeField] private Button _repairButton;
        [SerializeField] private TMP_Text _repairCostText;

        /// <summary>
        /// 업그레이드 버튼. Presenter가 onClick을 구독한다.
        /// </summary>
        public Button UpgradeButton
        {
            get { return _upgradeButton; }
        }

        /// <summary>
        /// 수리 버튼. Presenter가 onClick을 구독한다.
        /// </summary>
        public Button RepairButton
        {
            get { return _repairButton; }
        }

        /// <summary>
        /// 상태 문구를 바꾼다 (Presenter가 값이 바뀔 때만 호출한다).
        /// </summary>
        /// <param name="text">상태 문구</param>
        public void SetInfoText(string text)
        {
            if (_infoText != null)
            {
                _infoText.text = text;
            }
        }

        /// <summary>
        /// 업그레이드 버튼을 보이기·숨기고, 누를 수 있는지와 비용 문구를 맞춘다.
        /// </summary>
        /// <param name="isVisible">보이면 true (타워가 없거나 최대 단계면 숨긴다)</param>
        /// <param name="isInteractable">누를 수 있으면 true</param>
        /// <param name="costText">비용 문구 (바뀔 때만 넘긴다. null이면 그대로)</param>
        public void SetUpgrade(bool isVisible, bool isInteractable, string costText)
        {
            SetButton(_upgradeButton, _upgradeCostText, isVisible, isInteractable, costText);
        }

        /// <summary>
        /// 수리 버튼을 보이기·숨기고, 누를 수 있는지와 비용 문구를 맞춘다.
        /// </summary>
        /// <param name="isVisible">보이면 true (타워가 없으면 숨긴다)</param>
        /// <param name="isInteractable">누를 수 있으면 true (HP가 가득이거나 골드가 모자라면 막는다)</param>
        /// <param name="costText">비용 문구 (바뀔 때만 넘긴다. null이면 그대로)</param>
        public void SetRepair(bool isVisible, bool isInteractable, string costText)
        {
            SetButton(_repairButton, _repairCostText, isVisible, isInteractable, costText);
        }

        private static void SetButton(Button button, TMP_Text costLabel, bool isVisible, bool isInteractable, string costText)
        {
            if (button == null)
            {
                return;
            }
            if (button.gameObject.activeSelf != isVisible)
            {
                button.gameObject.SetActive(isVisible);
            }
            if (button.interactable != isInteractable)
            {
                button.interactable = isInteractable;
            }
            if (costLabel != null && costText != null)
            {
                costLabel.text = costText;
            }
        }
    }
}
