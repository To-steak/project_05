namespace Project05.Component.Data
{
    /// <summary>
    /// 난이도 증가 계수. DifficultyDefinition에서 베이킹되며, 적 스폰 시 1회 계산에 쓴다 (DESIGN 3.4.1).
    /// </summary>
    public struct DifficultyStats
    {
        // 적 HP 지수 증가 계수 (baseHP × hpGrowth^(w - 1))
        public float hpGrowth;
        // 드롭 골드 선형 증가 계수 (baseGold × (1 + goldGrowth × (w - 1)))
        public float goldGrowth;
        // 드롭 XP 선형 증가 계수 (baseXP × (1 + xpGrowth × (w - 1)))
        public float xpGrowth;
        // 보스 HP 배율
        public float bossHPMultiplier;
        // 보스 드롭 골드 배율
        public float bossGoldMultiplier;
        // 보스 드롭 XP 배율
        public float bossXPMultiplier;
    }
}
