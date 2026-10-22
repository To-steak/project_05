[Core]
네임스페이스: Project05.Core

DDOL Runner, 부트스트랩, App 기능, 베이킹. 모든 어셈블리를 참조할 수 있다.

들어갈 파일:
- Bootstrap.cs (static class) : RuntimeInitializeOnLoadMethod(BeforeSceneLoad)에서 Runner 생성, App 채우기, Definition 베이킹. SubsystemRegistration에서 static 초기화.
- Runner.cs (MonoBehaviour) : DDOL, 실행 순서 기본값(0, CinemachineBrain 100보다 먼저). FixedUpdate: System Tick 1~9 (일시정지 시 CommandSystem만, Defeat 시 정지). Update: 입력 수집. LateUpdate: ViewSystem → Presenter → 프레임 기록 비우기.
- App.cs (static class) : Data, Settings, Records, Scene, Audio, Pool 진입점.
- SettingsStore.cs (class) : 설정 PlayerPrefs 저장/로드.
- RecordStore.cs (class) : 최고 점수·최고 도달 웨이브 PlayerPrefs 저장/로드. Defeat 전환 시 갱신.
- SceneLoader.cs (class) : 비동기 씬 로드, allowSceneActivation 제어.
- AudioPlayer.cs (class) : BGM·SFX 재생, AudioSource 관리.
- ViewPool.cs (class) : UnityEngine.Pool.ObjectPool<T> 기반 View 풀. Game 씬 진입·퇴장 시 비움.
- InputCollector.cs (class) : Input System에서 좌클릭·Space를 읽어 클릭 레이캐스트 후 world.Input에 Command 적재.

구현하면서 이 목록과 실제 파일을 맞춘다. 컨벤션은 저장소 루트의 CONVENTIONS.md를 따른다.
