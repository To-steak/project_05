using Project05.Component;

namespace Project05.System
{
    /// <summary>
    /// [Tick 1] 입력 버퍼를 소비해 이동·탑승·하차·사격·건설·코어 UI 명령으로 해석한다. 일시정지 중에도 실행되며, 이때는 열린 UI의 명령만 처리하고 나머지는 버린다.
    /// 구현 브랜치: feat/hero-movement
    /// </summary>
    public static class CommandSystem
    {
        /// <summary>
        /// 입력 버퍼의 명령을 모두 꺼내 처리한다.
        /// </summary>
        /// <param name="world">현재 판의 월드</param>
        public static void Tick(World world)
        {
        }
    }
}
