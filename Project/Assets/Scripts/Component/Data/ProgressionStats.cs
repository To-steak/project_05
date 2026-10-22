namespace Project05.Component.Data
{
    /// <summary>
    /// 레벨 진행 테이블. ProgressionDefinition에서 베이킹된다.
    /// </summary>
    public struct ProgressionStats
    {
        // 레벨별 다음 레벨까지 필요한 XP (인덱스 = 현재 레벨 - 1)
        public int[] requiredXPByLevel;
    }
}
