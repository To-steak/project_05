using Project05.Component;

namespace Project05.System
{
    /// <summary>
    /// 새 판의 World를 초기화한다. Tick 없이 Runner가 판 시작 시 한 번 호출한다.
    /// 저장소 생성, 히어로·코어 설정, 베이킹 테이블 참조 연결은 해당 기능 브랜치에서 추가한다.
    /// </summary>
    public static class SessionSystem
    {
        /// <summary>
        /// 막 만든 World를 판 시작 상태(1-1 준비 단계)로 초기화한다.
        /// </summary>
        /// <param name="world">new World()로 만든 월드</param>
        public static void Initialize(World world)
        {
            world.tickCount = 0;
            world.pauseReason = PauseReason.None;

            ref GameFlowState flow = ref world.flow;
            flow.phase = GamePhase.Preparing;
            flow.stageNumber = 1;
            flow.waveNumber = 1;
        }

        /// <summary>
        /// 이번 틱에 System들을 실행해야 하는지 판단한다. 패배 시 전부 멈추고, 일시정지 중에는 CommandSystem만 실행한다.
        /// </summary>
        /// <param name="world">현재 판의 월드</param>
        /// <returns>CommandSystem 이후의 System을 실행해야 하면 true</returns>
        public static bool IsSimulationRunning(World world)
        {
            return world.flow.phase != GamePhase.Defeat && world.pauseReason == PauseReason.None;
        }

        /// <summary>
        /// 한 틱의 System 실행이 끝났을 때 시뮬레이션 틱 수를 올린다. Runner가 FixedUpdate 마지막에 호출한다.
        /// </summary>
        /// <param name="world">현재 판의 월드</param>
        public static void AdvanceTick(World world)
        {
            world.tickCount++;
        }
    }
}
