[Core]
네임스페이스: Project05.Core

DDOL Runner, 부트스트랩, App 기능, 베이킹. 모든 어셈블리를 참조할 수 있다.

들어갈 파일:
- Bootstrap.cs (static class) : RuntimeInitializeOnLoadMethod(BeforeSceneLoad)에서 Runner 생성, App 채우기, 데이터 베이킹. SubsystemRegistration에서 static 초기화.
- Runner.cs (MonoBehaviour) : DDOL. FixedUpdate: System Tick 순서 호출. Update: 입력 수집, 이벤트 배달.
- App.cs (static class) : Data, Settings, Scene, Audio, Pool 진입점.
- SettingsStore.cs (class) : PlayerPrefs 저장/로드.
- SceneLoader.cs (class) : 비동기 씬 로드, allowSceneActivation 제어.
- AudioPlayer.cs (class) : BGM·SFX 재생, AudioSource 관리.
- ViewPool.cs (class) : View GameObject 풀.
- InputCollector.cs (class) : Input System에서 입력을 읽어 클릭 레이캐스트 후 world.Input에 Command 적재.
- EventDispatcher.cs (class) : world.Events 큐를 비우며 ViewEvents 호출.

구현하면서 이 목록과 실제 파일을 맞춘다. 컨벤션은 저장소 루트의 CONVENTIONS.md를 따른다.
