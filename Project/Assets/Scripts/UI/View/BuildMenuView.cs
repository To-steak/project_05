using UnityEngine;
using UnityEngine.UI;

namespace Project05.UI.View
{
    /// <summary>
    /// [Game] 공터 건설 목록. 해금된 타워 종류마다 한 칸(BuildMenuEntryView)과 닫기 버튼. 표시만 담당하며 World와 App을 모른다.
    /// 이 컴포넌트는 항상 켜진 오브젝트(예: Canvas)에 두고, 실제 패널은 _panelRoot로 켜고 끈다.
    /// 칸은 _entryTemplate(꺼 둔 칸 하나)을 _entryContainer 아래에 복제해 판 시작 시 한 번 만든다 (게임 루프 중 생성 없음).
    /// </summary>
    public class BuildMenuView : MonoBehaviour
    {
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private BuildMenuEntryView _entryTemplate;
        [SerializeField] private Transform _entryContainer;
        [SerializeField] private Button _closeButton;

        /// <summary>
        /// 닫기 버튼. Presenter가 onClick을 구독한다.
        /// </summary>
        public Button CloseButton
        {
            get { return _closeButton; }
        }

        /// <summary>
        /// 템플릿을 복제해 칸을 만든다. 판 시작 시 Presenter가 한 번 호출한다. 템플릿이 없으면 빈 배열.
        /// </summary>
        /// <param name="count">만들 칸 수 (타워 종류 수)</param>
        /// <returns>만든 칸 (인덱스 = 타워 종류)</returns>
        public BuildMenuEntryView[] CreateEntries(int count)
        {
            if (_entryTemplate == null)
            {
                return new BuildMenuEntryView[0];
            }

            _entryTemplate.gameObject.SetActive(false);
            Transform container = _entryContainer != null ? _entryContainer : _entryTemplate.transform.parent;
            BuildMenuEntryView[] entries = new BuildMenuEntryView[count];
            for (int entryIndex = 0; entryIndex < count; entryIndex++)
            {
                entries[entryIndex] = Instantiate(_entryTemplate, container);
            }
            return entries;
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
