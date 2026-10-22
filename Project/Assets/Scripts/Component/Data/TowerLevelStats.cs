namespace Project05.Component.Data
{
    /// <summary>
    /// 타워 한 단계의 수치. TowerStats.levels의 원소다.
    /// </summary>
    public struct TowerLevelStats
    {
        // 이 단계의 최대 체력
        public int maxHP;
        // 이 단계의 공격력 (미탑승 자동 공격)
        public int attackDamage;
        // 이 단계의 사거리
        public float range;
        // 이 단계의 공격 주기 (틱)
        public int attackIntervalTicks;
        // 이 단계에서 다음 단계로 올리는 비용 (마지막 단계는 사용하지 않음)
        public int upgradeCost;
    }
}
