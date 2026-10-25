[Foundation]
네임스페이스: Project05.Foundation

프로그램 전체 수명의 기반 기능. static App과 그 기능 클래스. Bootstrap(Core)이 App을 채운다. 어셈블리 참조: Component.
View·UI·Core가 참조한다. System은 참조하지 않는다 (System에서 App을 쓰면 컴파일 에러).
Core에 두면 View·UI → Core → View·UI 순환 참조가 되므로 별도 어셈블리로 둔다. 폴더 이름을 App으로 하지 않는다 (CONVENTIONS 1.1).

들어갈 파일:
- App.cs (static class) : Data, ViewData, Settings, Records, Scene, Audio, Pool 진입점.
- ViewData.cs (class) : View용 테이블 (적 프리팹, 히어로 투사체·구체 프리팹, 적 사망 효과, VisualEffectKind별 효과 프리팹, 획득 문구 프리팹·형식 등). DefinitionBaker.BakeView가 채운다. System이 GameObject를 모르도록 GameData와 분리.
- SettingsStore.cs (class) : 설정 PlayerPrefs 저장/로드. 지금은 BGM·SFX 볼륨(0 ~ 1). 디스크 기록은 Save(설정 패널 닫을 때).
- RecordStore.cs (class) : 최고 점수·최고 도달 웨이브 PlayerPrefs 저장/로드. Defeat 전환 시 갱신.
- SceneLoader.cs (class) : Main → Game 비동기 로드(allowSceneActivation false, 0.9에서 준비, 시작 버튼에 활성화). 결과 패널용 Game·Main 동기 로드.
- AudioPlayer.cs (class) : 배경음·효과음 재생. Runner 오브젝트의 AudioSource(배경음 1 + 효과음 동시 재생 수)를 받는다. 매 프레임 SynchronizeVolumes로 설정 볼륨 반영. WebGL은 첫 사용자 입력(Unlock) 전까지 잠김. 같은 효과음은 한 프레임에 한 번.
- AudioData.cs (class) : 오디오 테이블 (종류 enum 인덱스 클립 배열, 믹스 배율, 동시 재생 수). DefinitionBaker.BakeAudio가 채운다.
- SoundEffectKind.cs (enum) : 효과음 종류. 대부분 프레임 기록 하나에 대응.
- VisualEffectKind.cs (enum) : 한 번짜리 시각 효과 종류 (ViewData.visualEffectPrefabs 인덱스). 적 사망은 종류별이라 제외.
- BackgroundMusicKind.cs (enum) : 배경음 종류 (None, MainMenu, Game, Boss).
- ViewPool.cs (class) : UnityEngine.Pool.ObjectPool<T> 기반 프리팹별 View 풀. 인스턴스 → 풀 매핑으로 반납. Game 씬 진입·퇴장 시 Runner가 비움.

구현하면서 이 목록과 실제 파일을 맞춘다. 컨벤션은 저장소 루트의 CONVENTIONS.md를 따른다.
