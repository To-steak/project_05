namespace Project05.Component.Entity
{
    /// <summary>
    /// 히어로 공격 종류. 공격 타이머가 준비된 틱에 거리에 따라 하나를 고른다.
    /// </summary>
    public enum HeroAttackKind
    {
        // 근거리 광역 베기
        Melee,
        // 원거리 단일 공격
        Ranged,
    }
}
