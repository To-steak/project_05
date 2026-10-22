[Foundation]
네임스페이스: Project05.Foundation

프로그램 전체 수명의 기반 기능. static App과 그 기능 클래스. Bootstrap(Core)이 App을 채운다. 어셈블리 참조: Component.
View·UI·Core가 참조한다. System은 참조하지 않는다 (System에서 App을 쓰면 컴파일 에러).
Core에 두면 View·UI → Core → View·UI 순환 참조가 되므로 별도 어셈블리로 둔다. 폴더 이름을 App으로 하지 않는다 (CONVENTIONS 1.1).

들어갈 파일:
- App.cs (static class) : Data, Settings, Records, Scene, Audio, Pool 진입점.
- SettingsStore.cs (class) : 설정 PlayerPrefs 저장/로드.
- RecordStore.cs (class) : 최고 점수·최고 도달 웨이브 PlayerPrefs 저장/로드. Defeat 전환 시 갱신.
- SceneLoader.cs (class) : 비동기 씬 로드, allowSceneActivation 제어.
- AudioPlayer.cs (class) : BGM·SFX 재생, AudioSource 관리.
- ViewPool.cs (class) : UnityEngine.Pool.ObjectPool<T> 기반 View 풀. Game 씬 진입·퇴장 시 비움.

구현하면서 이 목록과 실제 파일을 맞춘다. 컨벤션은 저장소 루트의 CONVENTIONS.md를 따른다.
