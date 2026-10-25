using UnityEngine;

namespace Project05.View
{
    /// <summary>
    /// 코어 피격 화면 경고 (화면 가장자리 붉은 테두리 등). Overlay Canvas 아래 이미지 묶음에 CanvasGroup과 함께 붙인다. 투명도만 바꾼다. 로직 없음.
    /// CoreViewSystem이 코어가 맞을 때마다 세기를 1로 올리고, 유지 시간 동안 0으로 줄인다.
    /// </summary>
    public class CoreWarningView : MonoBehaviour
    {
        // 경고 이미지 묶음의 CanvasGroup (투명도 조절, 클릭을 막지 않도록 raycast는 끈다)
        [SerializeField] private CanvasGroup _canvasGroup;
        // 한 번 맞은 뒤 경고가 사라지기까지 시간 (틱, 1틱 = 20ms)
        [SerializeField, Min(1)] private int _durationTicks = 30;
        // 세기 1일 때 투명도
        [SerializeField, Range(0f, 1f)] private float _maxAlpha = 0.6f;

        /// <summary>
        /// 한 번 맞은 뒤 경고가 사라지기까지 시간 (틱)
        /// </summary>
        public int DurationTicks
        {
            get { return _durationTicks; }
        }

        /// <summary>
        /// 경고 세기를 바꾼다. 0이면 보이지 않는다.
        /// </summary>
        /// <param name="intensity">세기 (0 ~ 1)</param>
        public void SetIntensity(float intensity)
        {
            if (_canvasGroup == null)
            {
                return;
            }

            _canvasGroup.alpha = Mathf.Clamp01(intensity) * _maxAlpha;
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;
        }
    }
}
