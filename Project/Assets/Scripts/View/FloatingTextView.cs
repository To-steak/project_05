using TMPro;
using UnityEngine;

namespace Project05.View
{
    /// <summary>
    /// 드롭 획득 "+10G" 같은 월드 공간 떠오르는 문구 프리팹의 루트 컴포넌트. 문구·진행도 표시만 한다. 로직 없음.
    /// VisualEffectViewSystem이 진행도(0 ~ 1)를 넘기면 위로 떠오르며 뒤쪽 구간에서 흐려진다.
    /// </summary>
    public class FloatingTextView : MonoBehaviour
    {
        // 문구를 그릴 TextMeshPro (월드 공간 TextMeshPro 권장)
        [SerializeField] private TMP_Text _text;
        // 풀에 돌아가기까지 유지 시간 (틱, 1틱 = 20ms)
        [SerializeField, Min(1)] private int _lifetimeTicks = 40;
        // 유지 시간 동안 떠오르는 높이 (월드 유닛)
        [SerializeField, Min(0f)] private float _riseDistance = 1.5f;
        // 흐려지기 시작하는 진행도 (0 ~ 1. 이전에는 불투명)
        [SerializeField, Range(0f, 1f)] private float _fadeStartProgress = 0.5f;

        /// <summary>
        /// 풀에 돌아가기까지 유지 시간 (틱)
        /// </summary>
        public int LifetimeTicks
        {
            get { return _lifetimeTicks; }
        }

        /// <summary>
        /// 유지 시간 동안 떠오르는 높이 (월드 유닛)
        /// </summary>
        public float RiseDistance
        {
            get { return _riseDistance; }
        }

        /// <summary>
        /// 표시할 문구를 바꾼다.
        /// </summary>
        /// <param name="text">표시할 문구</param>
        public void SetText(string text)
        {
            if (_text != null)
            {
                _text.text = text;
            }
        }

        /// <summary>
        /// 진행도에 맞춰 투명도를 바꾼다. 흐려지기 시작하는 진행도 전에는 불투명, 끝에서 완전히 투명하다.
        /// </summary>
        /// <param name="progress">진행도 (0 = 막 나옴, 1 = 끝)</param>
        public void SetProgress(float progress)
        {
            if (_text == null)
            {
                return;
            }

            float fadeLength = 1f - _fadeStartProgress;
            float alpha = fadeLength <= 0f ? 1f : 1f - Mathf.Clamp01((progress - _fadeStartProgress) / fadeLength);
            _text.alpha = alpha;
        }
    }
}
