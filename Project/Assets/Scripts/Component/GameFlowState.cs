namespace Project05.Component
{
    /// <summary>
    /// 게임 흐름 상태. 단계, 스테이지-웨이브, 처치 진행도, 보스를 담는다.
    /// </summary>
    public struct GameFlowState
    {
        // 현재 게임 단계
        public GamePhase phase;
        // 스테이지 번호 n (1부터)
        public int stageNumber;
        // 스테이지 안의 웨이브 번호 x (1 ~ 10)
        public int waveNumber;
        // 이번 웨이브 처치 수 (진행도 current)
        public int killCount;
        // 이번 웨이브 적 수 (진행도 max = x × 10)
        public int waveEnemyCount;
        // 이번 웨이브에서 생성 요청한 적 수
        public int spawnedEnemyCount;
        // 다음 단계 전환까지 남은 틱 (준비 시간 등)
        public int phaseRemainingTicks;
        // 다음 적 스폰까지 남은 틱
        public int spawnCooldownTicks;
        // 등장한 보스 ID (보스가 없으면 무효 ID)
        public EntityID bossID;
    }
}
