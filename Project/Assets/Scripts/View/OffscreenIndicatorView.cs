using UnityEngine;

namespace Project05.View
{
    /// <summary>
    /// 화면 밖 적 방향 화살표 묶음. Screen Space - Overlay Canvas 아래에 두고, 위쪽(+Y)을 가리키는 화살표 하나(_arrowTemplate)를 자식으로 둔다.
    /// 판 시작 시 Runner가 찾아 화살표를 _maxArrowCount개 복제해 둔다 (게임 루프 중 생성 없음). 로직 없음.
    /// </summary>
    public class OffscreenIndicatorView : MonoBehaviour
    {
        [SerializeField] private RectTransform _arrowTemplate;
        // 동시에 보일 최대 화살표 수 (화면 밖 적 중 코어에 가까운 순)
        [SerializeField, Min(1)] private int _maxArrowCount = 8;
        // 화면 가장자리에서 화살표까지의 여백 (픽셀)
        [SerializeField, Min(0f)] private float _edgeMargin = 40f;

        /// <summary>
        /// 화면 가장자리에서 화살표까지의 여백 (픽셀)
        /// </summary>
        public float EdgeMargin
        {
            get { return _edgeMargin; }
        }

        /// <summary>
        /// 템플릿을 복제해 화살표를 만든다 (판 시작 시 1회). 템플릿은 숨긴다. 템플릿이 없으면 빈 배열.
        /// </summary>
        /// <returns>만든 화살표 (모두 꺼진 상태)</returns>
        public RectTransform[] CreateArrows()
        {
            if (_arrowTemplate == null)
            {
                return new RectTransform[0];
            }

            _arrowTemplate.gameObject.SetActive(false);
            RectTransform[] arrows = new RectTransform[_maxArrowCount];
            for (int arrowIndex = 0; arrowIndex < arrows.Length; arrowIndex++)
            {
                arrows[arrowIndex] = Instantiate(_arrowTemplate, _arrowTemplate.parent);
            }
            return arrows;
        }
    }
}
