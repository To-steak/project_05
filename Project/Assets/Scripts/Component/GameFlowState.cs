namespace Project05.Component
{
    /// <summary>
    /// 게임 흐름 상태. 처치 수, 웨이브 적 수, 보스 ID 등은 feat/enemy-wave, feat/core-siege에서 추가한다.
    /// </summary>
    public struct GameFlowState
    {
        // 현재 게임 단계
        public GamePhase phase;
        // 스테이지 번호 n (1부터)
        public int stageNumber;
        // 스테이지 안의 웨이브 번호 x (1 ~ 10)
        public int waveNumber;
    }
}
