using UnityEngine;
using UnityEngine.UI;

namespace Project05.UI.View
{
    /// <summary>
    /// [Game] 코어 UI. 해금 탭(타워 종류별 칸)과 타워 탭(공터별 칸)을 탭 버튼으로 전환하고, 닫기 버튼이 있다.
    /// 표시만 담당하며 World와 App을 모른다. 이 컴포넌트는 항상 켜진 오브젝트에 두고, 실제 패널은 _panelRoot로 켜고 끈다.
    /// 칸은 템플릿(꺼 둔 칸 하나)을 컨테이너 아래에 복제해 판 시작 시 한 번 만든다.
    /// </summary>
    public class CoreMenuView : MonoBehaviour
    {
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private Button _unlockTabButton;
        [SerializeField] private Button _towerTabButton;
        [SerializeField] private GameObject _unlockTabRoot;
        [SerializeField] private GameObject _towerTabRoot;
        [SerializeField] private CoreUnlockEntryView _unlockEntryTemplate;
        [SerializeField] private Transform _unlockEntryContainer;
        [SerializeField] private CoreTowerEntryView _towerEntryTemplate;
        [SerializeField] private Transform _towerEntryContainer;
        [SerializeField] private Button _closeButton;

        /// <summary>
        /// 해금 탭 버튼. Presenter가 onClick을 구독한다.
        /// </summary>
        public Button UnlockTabButton
        {
            get { return _unlockTabButton; }
        }

        /// <summary>
        /// 타워 탭 버튼. Presenter가 onClick을 구독한다.
        /// </summary>
        public Button TowerTabButton
        {
            get { return _towerTabButton; }
        }

        /// <summary>
        /// 닫기 버튼. Presenter가 onClick을 구독한다.
        /// </summary>
        public Button CloseButton
        {
            get { return _closeButton; }
        }

        /// <summary>
        /// 해금 탭 칸을 만든다 (판 시작 시 1회). 템플릿이 없으면 빈 배열.
        /// </summary>
        /// <param name="count">타워 종류 수</param>
        /// <returns>만든 칸 (인덱스 = 타워 종류)</returns>
        public CoreUnlockEntryView[] CreateUnlockEntries(int count)
        {
            return CreateEntries(_unlockEntryTemplate, _unlockEntryContainer, count);
        }

        /// <summary>
        /// 타워 탭 칸을 만든다 (판 시작 시 1회). 템플릿이 없으면 빈 배열.
        /// </summary>
        /// <param name="count">공터 수</param>
        /// <returns>만든 칸 (인덱스 = 공터 번호)</returns>
        public CoreTowerEntryView[] CreateTowerEntries(int count)
        {
            return CreateEntries(_towerEntryTemplate, _towerEntryContainer, count);
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
        /// 보일 탭을 고른다. 고른 탭의 버튼은 누를 수 없게 해 현재 탭을 표시한다.
        /// </summary>
        /// <param name="isUnlockTab">해금 탭이면 true, 타워 탭이면 false</param>
        public void ShowTab(bool isUnlockTab)
        {
            if (_unlockTabRoot != null)
            {
                _unlockTabRoot.SetActive(isUnlockTab);
            }
            if (_towerTabRoot != null)
            {
                _towerTabRoot.SetActive(!isUnlockTab);
            }
            if (_unlockTabButton != null)
            {
                _unlockTabButton.interactable = !isUnlockTab;
            }
            if (_towerTabButton != null)
            {
                _towerTabButton.interactable = isUnlockTab;
            }
        }

        private static T[] CreateEntries<T>(T template, Transform container, int count) where T : MonoBehaviour
        {
            if (template == null)
            {
                return new T[0];
            }

            template.gameObject.SetActive(false);
            Transform parent = container != null ? container : template.transform.parent;
            T[] entries = new T[count];
            for (int entryIndex = 0; entryIndex < count; entryIndex++)
            {
                entries[entryIndex] = Instantiate(template, parent);
                entries[entryIndex].gameObject.SetActive(true);
            }
            return entries;
        }
    }
}
