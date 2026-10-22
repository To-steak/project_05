namespace Project05.Component.Entity
{
    /// <summary>
    /// 히어로 상태.
    /// </summary>
    public enum HeroState
    {
        // 지상에서 멈춰 있음
        Idle,
        // 지상에서 경로를 따라 이동 중
        Moving,
        // 타워에 탑승 중
        Boarded,
        // 사망 후 부활 대기
        Dead,
    }
}
