# CLAUDE.md

이 프로젝트는 Unity 6 (6000.3.11f1, URP), WebGL 타겟의 액션 타워 디펜스다. 게임 설계는 `DESIGN.md`를 기준으로 한다. 작업 전 반드시 읽을 것.

Unity 프로젝트 루트는 `Project/`다.

## 아키텍처 규칙 (반드시 지킬 것)

### Logic / View 분리
- **Logic은 FixedUpdate에서만 실행한다.** 게임 상태를 바꾸는 코드는 `FixedTick()` 안에만 둔다.
- **View는 Update / LateUpdate에서만 실행한다.** 애니메이션, SFX, VFX, UI가 View다.
- Logic은 View를 참조하지 않는다. 일어난 일을 이벤트로만 알린다.
- View는 Logic 상태를 읽고 이벤트를 구독할 뿐, Logic 상태를 직접 바꾸지 않는다.
- **Transform은 View만 쓴다.** Logic은 위치를 데이터(`prevPosition`, `currPosition`)로 소유하고, View가 보간해서 반영한다.

### 서비스
- 기능은 `~Service` 이름의 **순수 C# 클래스**로 만든다. MonoBehaviour로 만들지 않는다.
- 씬은 Main, Game 2개뿐이다. Boot 씬은 없다.
- 서비스는 `RuntimeInitializeOnLoadMethod(BeforeSceneLoad)`에서 1회 등록되고 DDOL Runner가 유지한다. 씬마다 새로 만들지 않는다.
- 서비스 접근은 `ServiceLocator`를 통한다.
- 로직 서비스의 틱 순서는 Runner가 관리한다. 순서는 `DESIGN.md` 5.4를 따른다.
- 판(세션)마다 초기화가 필요한 상태를 가진 서비스는 `IGameSessionListener`를 구현하고, 상태 초기화는 `OnSessionStart()`에서 한다.

### 입력
- 프로젝트는 새 Input System 전용이다. 구 `UnityEngine.Input` API를 쓰지 않는다.
- `wasPressedThisFrame` 등 프레임 단위 입력을 FixedUpdate에서 읽지 않는다.
- 입력은 InputService가 Update에서 버퍼링하고, Logic은 FixedTick에서 소비한다.

### 이동
- `NavMeshAgent`의 자동 이동을 쓰지 않는다. `NavMesh.CalculatePath`로 경로만 받고 이동은 FixedTick에서 직접 처리한다.
- 적은 웨이포인트를 따라 이동한다.

### 전투 · 연출
- 데미지 판정 시점은 Logic의 타이머가 결정한다. 애니메이션 이벤트로 데미지를 넣지 않는다.
- 적 사망 시 Logic 객체는 즉시 풀에 반환하고, 사망 연출은 별도로 재생한다.
- AudioService는 View 쪽에서만 호출한다.

### UI
- View-Presenter 구조. Presenter가 서비스 이벤트를 구독하고 View를 갱신한다.
- View는 서비스를 직접 참조하지 않는다.

## WebGL 주의
- `Instantiate` / `Destroy`를 게임 루프에서 반복하지 않는다. PoolService를 쓴다.
- 멀티스레드(`System.Threading`, `Task.Run`)를 쓰지 않는다.
- 매 틱 할당(LINQ, 클로저, 박싱, 문자열 결합)을 피한다.

## 작업 방식
- 설계에 없는 시스템이나 규칙을 새로 도입해야 하면 코드 작성 전에 먼저 물어본다.
- 수치는 하드코딩하지 않고 DataService(ScriptableObject)에서 읽는다.
- `DESIGN.md`의 미결정 사항에 해당하는 부분은 임의로 정하지 말고 확인한다.
