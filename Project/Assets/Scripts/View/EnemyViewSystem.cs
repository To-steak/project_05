using Project05.Component;

namespace Project05.View
{
    /// <summary>
    /// 적 동기화(새 ID는 풀에서 꺼내고 사라진 ID는 반납), 보간, 피격 플래시, 사망 VFX.
    /// Runner가 LateUpdate에서 정해진 순서로 호출한다. World는 읽기만 한다.
    /// 구현 브랜치: feat/enemy-wave, feat/hero-combat
    /// </summary>
    public static class EnemyViewSystem
    {
        /// <summary>
        /// 적 View를 동기화하고 적 기록을 연출한다.
        /// </summary>
        /// <param name="world">현재 판의 월드 (읽기 전용)</param>
        /// <param name="viewContext">현재 판의 화면 상태</param>
        public static void Present(World world, ViewContext viewContext)
        {
        }
    }
}
