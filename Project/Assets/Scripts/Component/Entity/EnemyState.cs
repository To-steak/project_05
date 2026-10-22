namespace Project05.Component.Entity
{
    /// <summary>
    /// 적 상태.
    /// </summary>
    public enum EnemyState
    {
        // 웨이포인트 경로를 따라 이동 중
        FollowingPath,
        // 타워 포위 자리에 도착해 타워를 공격 중
        AttackingTower,
        // 배정받은 포위 자리(코어, 또는 targetTowerID 타워 주변)로 걸어가는 중
        MovingToSiegeSlot,
        // 코어 포위 자리에 도착해 코어를 공격 중
        AttackingCore,
    }
}
