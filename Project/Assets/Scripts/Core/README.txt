[Core]
네임스페이스: Project05.Core

DDOL Runner, 부트스트랩, 베이킹, 입력 수집. 모든 어셈블리를 참조할 수 있다. App과 그 기능은 Foundation/에 있다.

들어갈 파일:
- Bootstrap.cs (static class) : RuntimeInitializeOnLoadMethod(BeforeSceneLoad)에서 Runner 생성, App 채우기, Definition 베이킹, Runner 오브젝트에 AudioSource를 붙여 App.Audio 생성(WebGL은 잠금 상태로). SubsystemRegistration에서 static 초기화.
- Runner.cs (MonoBehaviour) : DDOL, 실행 순서 기본값(0, CinemachineBrain 100보다 먼저). FixedUpdate: System Tick 1~9 (일시정지 시 CommandSystem만, Defeat 시 정지). Update: 첫 입력 시 오디오 잠금 해제, 입력 수집. LateUpdate: 볼륨 반영, ViewSystem(효과를 꺼내는 ViewSystem → VisualEffectViewSystem → Camera → Audio) → Presenter → 프레임 기록 비우기. Main 씬 진입 시 메뉴 배경음.
- InputCollector.cs (class) : Input System에서 좌클릭·Space를 읽어 클릭 레이캐스트 후 world.Input에 Command 적재. 오디오 잠금 해제용 '아무 클릭·키를 뗐는지' 확인.

구현하면서 이 목록과 실제 파일을 맞춘다. 컨벤션은 저장소 루트의 CONVENTIONS.md를 따른다.
