using Project05.Component;

namespace Project05.View
{
    /// <summary>
    /// 히어로 상호작용 범위 안의 대상(공터 / 타워 / 코어)에 Space 아이콘 표시.
    /// Runner가 LateUpdate에서 정해진 순서로 호출한다. World는 읽기만 한다.
    /// 구현 브랜치: 4단계
    /// </summary>
    public static class InteractionPromptViewSystem
    {
        /// <summary>
        /// 상호작용 가능한 대상 위에 Space 아이콘을 표시한다.
        /// </summary>
        /// <param name="world">현재 판의 월드 (읽기 전용)</param>
        /// <param name="viewContext">현재 판의 화면 상태</param>
        public static void Present(World world, ViewContext viewContext)
        {
        }
    }
}
