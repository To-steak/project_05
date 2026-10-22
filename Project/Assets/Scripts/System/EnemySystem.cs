using Project05.Component;

namespace Project05.System
{
    /// <summary>
    /// [Tick 6] 적 경로 이동, aggroRange 안 타워 우선 공격, 코어 포위·공격, 사망 처리(드롭 생성 요청, 처치 수 증가), 타워 파괴(공터 비움, 탑승 중이면 강제 하차)를 처리한다.
    /// 구현 브랜치: feat/enemy-wave, feat/hero-combat, feat/core-siege, feat/tower
    /// </summary>
    public static class EnemySystem
    {
        /// <summary>
        /// 모든 적을 한 틱 진행한다.
        /// </summary>
        /// <param name="world">현재 판의 월드</param>
        public static void Tick(World world)
        {
        }
    }
}
