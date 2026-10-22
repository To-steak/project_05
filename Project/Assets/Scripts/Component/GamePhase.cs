namespace Project05.Component
{
    /// <summary>
    /// 게임 단계. 무한 스테이지이므로 승리 단계는 없다.
    /// </summary>
    public enum GamePhase
    {
        // 웨이브 사이 준비 시간
        Preparing,
        // 일반 웨이브 진행 중 (n-1 ~ n-9, n-10의 보스 등장 전)
        WaveInProgress,
        // 보스 등장 후 처치 전
        BossInProgress,
        // 패배. 모든 System 실행을 멈춘다
        Defeat,
    }
}
