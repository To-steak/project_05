namespace Project05.Component.Request
{
    /// <summary>
    /// 적·보스 생성 요청. SpawnSystem이 테이블 조회와 난이도 계산 후 적을 만든다.
    /// </summary>
    public struct EnemySpawnRequest
    {
        // 적 종류 (EnemyStats 테이블 인덱스)
        public int enemyTypeIndex;
        // 스폰 지점 인덱스 (World.spawnPoints)
        public int spawnPointIndex;
        // 누적 웨이브 번호 w = (n - 1) × 10 + x
        public int cumulativeWaveNumber;
        // 보스 여부
        public bool isBoss;
    }
}
