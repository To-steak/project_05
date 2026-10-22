using Project05.Component;

namespace Project05.View
{
    /// <summary>
    /// 타워 동기화, 사격·피격·파괴·수리·업그레이드 연출, 탑승 연출.
    /// Runner가 LateUpdate에서 정해진 순서로 호출한다. World는 읽기만 한다.
    /// 구현 브랜치: feat/tower
    /// </summary>
    public static class TowerViewSystem
    {
        /// <summary>
        /// 타워 View를 동기화하고 타워 기록을 연출한다.
        /// </summary>
        /// <param name="world">현재 판의 월드 (읽기 전용)</param>
        /// <param name="viewContext">현재 판의 화면 상태</param>
        public static void Present(World world, ViewContext viewContext)
        {
        }
    }
}
