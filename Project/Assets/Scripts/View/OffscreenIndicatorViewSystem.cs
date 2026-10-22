using Project05.Component;

namespace Project05.View
{
    /// <summary>
    /// 화면 밖에서 코어로 다가오는 적 방향 화살표.
    /// Runner가 LateUpdate에서 정해진 순서로 호출한다. World는 읽기만 한다.
    /// 구현 브랜치: 4단계
    /// </summary>
    public static class OffscreenIndicatorViewSystem
    {
        /// <summary>
        /// 화면 밖 적의 방향 화살표를 갱신한다.
        /// </summary>
        /// <param name="world">현재 판의 월드 (읽기 전용)</param>
        /// <param name="viewContext">현재 판의 화면 상태</param>
        public static void Present(World world, ViewContext viewContext)
        {
        }
    }
}
