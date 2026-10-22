using Project05.Component;

namespace Project05.System
{
    /// <summary>
    /// [Tick 2] 준비 / 웨이브 / 보스 / 패배 단계를 전이한다. 패배 판정(코어 HP 0, 히어로 목숨 0)을 여기서 한 곳에 모아 한다.
    /// 구현 브랜치: feat/core-siege
    /// </summary>
    public static class GameFlowSystem
    {
        /// <summary>
        /// 단계 전환 조건을 확인하고 게임 흐름을 갱신한다.
        /// </summary>
        /// <param name="world">현재 판의 월드</param>
        public static void Tick(World world)
        {
        }
    }
}
