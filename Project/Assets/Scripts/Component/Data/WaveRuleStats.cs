namespace Project05.Component.Data
{
    /// <summary>
    /// 웨이브 규칙 테이블. WaveRuleDefinition에서 베이킹된다. 적 구성 생성 규칙은 미정.
    /// </summary>
    public struct WaveRuleStats
    {
        // 웨이브 적 수 계수. 웨이브 적 수 max = x × 이 값 (임시 규칙: 10)
        public int enemyCountPerWaveNumber;
        // 적 스폰 간격 (틱)
        public int spawnIntervalTicks;
        // 웨이브 사이 준비 시간 (틱)
        public int preparationTicks;
        // 보스 웨이브 번호이자 스테이지당 웨이브 수 (10)
        public int bossWaveNumber;
    }
}
