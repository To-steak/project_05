using Project05.Component.Data;

namespace Project05.Foundation
{
    /// <summary>
    /// 프로그램 전체 수명의 기능 진입점. View, UI, Core만 쓴다 (System 어셈블리는 Foundation을 참조하지 않는다).
    /// 모든 필드는 Bootstrap(Core)이 게임 시작 시 채운다.
    /// </summary>
    public static class App
    {
        // 베이킹된 struct 테이블, 읽기 전용 (DefinitionBaker가 채운다, feat/baking)
        public static GameData Data;
        // View용 테이블 (프리팹 등), 읽기 전용 (DefinitionBaker가 채운다)
        public static ViewData ViewData;
        // View GameObject 풀 (Runner가 Game 씬 진입·퇴장 시 비운다)
        public static ViewPool Pool;
        // 씬 로더 (로딩 패널, 결과 패널 버튼)
        public static SceneLoader Scene;
        // 설정 PlayerPrefs 저장/로드
        public static SettingsStore Settings;
        // 최고 점수·최고 도달 웨이브 PlayerPrefs 저장/로드
        public static RecordStore Records;
        // 배경음·효과음 재생 (AudioSource는 Runner 오브젝트에 붙어 있다)
        public static AudioPlayer Audio;

        /// <summary>
        /// 모든 static 값을 비운다. 도메인 리로드를 끈 Play에 대비해 SubsystemRegistration에서 호출한다.
        /// </summary>
        public static void Reset()
        {
            Data = null;
            ViewData = null;
            Pool = null;
            Scene = null;
            Settings = null;
            Records = null;
            Audio = null;
        }
    }
}
