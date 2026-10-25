namespace Project05.View
{
    /// <summary>
    /// 재생 중인 효과 하나 (ViewContext.activeVisualEffects의 원소). 풀에 돌려줄 시각을 함께 든다.
    /// </summary>
    public struct ActiveVisualEffect
    {
        // 풀에서 꺼낸 효과
        public VisualEffectView effectView;
        // 풀에 돌려줄 Time.time
        public float endTime;
    }
}
