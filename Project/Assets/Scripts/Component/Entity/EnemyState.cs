namespace Project05.Component.Entity
{
    /// <summary>
    /// 적 상태.
    /// </summary>
    public enum EnemyState
    {
        // 웨이포인트 경로를 따라 이동 중
        FollowingPath,
        // aggroRange 안의 타워를 공격 중
        AttackingTower,
        // 코어를 포위하고 공격 중
        AttackingCore,
    }
}
