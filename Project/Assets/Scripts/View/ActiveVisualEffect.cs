namespace Project05.View
{
    /// <summary>
    /// 재생 중인 효과 하나 (ViewContext.activeVisualEffects의 원소). 끝났는지는 매 프레임 효과의 파티클에 묻는다.
    /// </summary>
    public struct ActiveVisualEffect
    {
        // 풀에서 꺼낸 효과
        public VisualEffectView effectView;
    }
}
