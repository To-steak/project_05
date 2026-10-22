using Project05.Component;

namespace Project05.View
{
    /// <summary>
    /// Cinemachine 추적 대상(히어로 View Transform) 연결, 탑승 시 카메라 전환. 히어로 보간 이후에 호출되어야 한다.
    /// Runner가 LateUpdate에서 정해진 순서로 호출한다. World는 읽기만 한다.
    /// 구현 브랜치: feat/hero-movement
    /// </summary>
    public static class CameraViewSystem
    {
        /// <summary>
        /// 카메라 추적 대상과 전환 상태를 갱신한다.
        /// </summary>
        /// <param name="world">현재 판의 월드 (읽기 전용)</param>
        /// <param name="viewContext">현재 판의 화면 상태</param>
        public static void Present(World world, ViewContext viewContext)
        {
        }
    }
}
