namespace Project05.Component
{
    /// <summary>
    /// 한 판의 모든 게임 상태. 판마다 new World()로 만들고 SessionSystem.Initialize로 초기화한다.
    /// 엔티티 저장소, 히어로, 코어, 입력 버퍼, 생성 요청 큐, 프레임 기록 목록은 해당 기능 브랜치에서 추가한다.
    /// </summary>
    public class World
    {
        // 판 시작 후 실행된 시뮬레이션 틱 수 (일시정지·패배 중에는 오르지 않음)
        public int tickCount;
        // 게임 흐름 (단계, 스테이지, 웨이브)
        public GameFlowState flow;
        // 일시정지 사유. None이 아니면 CommandSystem만 실행
        public PauseReason pauseReason;
    }
}
