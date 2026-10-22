using Project05.Component.Data;

namespace Project05.Core
{
    /// <summary>
    /// 프로그램 전체 수명의 기능 진입점. View, UI, Core만 쓴다 (System은 사용 금지).
    /// Settings, Records, Scene, Audio, Pool은 해당 기능 브랜치에서 필드로 추가하고 Bootstrap이 채운다.
    /// </summary>
    public static class App
    {
        // 부트스트랩이 만든 DDOL Runner
        public static Runner Runner;
        // 베이킹된 struct 테이블, 읽기 전용 (DefinitionBaker가 채운다, feat/baking)
        public static GameData Data;

        /// <summary>
        /// 모든 static 값을 비운다. 도메인 리로드를 끈 Play에 대비해 SubsystemRegistration에서 호출한다.
        /// </summary>
        public static void Reset()
        {
            Runner = null;
            Data = null;
        }
    }
}
