using UnityEngine;

namespace Project05.View
{
    /// <summary>
    /// 떠오르는 중인 문구 하나 (ViewContext.activeFloatingTexts의 원소).
    /// </summary>
    public struct ActiveFloatingText
    {
        // 풀에서 꺼낸 문구
        public FloatingTextView textView;
        // 나온 위치 (여기서 위로 떠오른다)
        public Vector3 startPosition;
        // 나온 Time.time
        public float startTime;
        // 풀에 돌려줄 Time.time
        public float endTime;
    }
}
