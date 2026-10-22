# CLAUDE.md

이 프로젝트는 Unity 6 (6000.3.11f1, URP), WebGL 타겟의 액션 타워 디펜스다. 게임 설계는 `DESIGN.md`, 코드 컨벤션은 `CONVENTIONS.md`를 기준으로 한다. 코드 작성 전 두 문서를 반드시 읽을 것.

Unity 프로젝트 루트는 `Project/`다.

## 아키텍처 규칙 (반드시 지킬 것)

ECS 패키지(DOTS)는 쓰지 않는다. "System은 함수, 상태는 데이터"라는 철학만 순수 C#으로 따른다. 상세는 `DESIGN.md` 5장.

### Logic / View 분리
- **Logic은 FixedUpdate에서만 실행한다.** 게임 상태를 바꾸는 코드는 System의 `Tick(World)` 안에만 둔다.
- **View는 Update / LateUpdate에서만 실행한다.** 애니메이션, SFX, VFX, UI, 카메라가 View다.
- Logic은 View를 참조하지 않는다. 일어난 일은 `world.Events` 큐에 남긴다.
- View는 World를 읽고 이벤트를 받을 뿐, World를 직접 바꾸지 않는다. 입력은 `world.Input` 버퍼를 거친다.
- **Transform은 View만 쓴다.** Logic은 위치를 데이터(`previousPosition`, `currentPosition`)로 소유하고, View가 보간해서 반영한다.

### World / System / App
- 한 판의 모든 상태는 **World**에 둔다. 판마다 `new World()`로 새로 만든다.
- 로직은 `~System` 이름의 **static 클래스**로 만든다. **static 필드에 게임 상태를 두지 않는다.** 시그니처는 `public static void Tick(World world)`.
- System 간 호출은 직접 호출한다. 틱 순서는 Runner가 관리하며 `DESIGN.md` 5.5를 따른다.
- 입력 처리 System 이름은 `CommandSystem`이다 (`UnityEngine.InputSystem`과 충돌 방지).
- 프로그램 전체 수명의 기능은 **static `App`**(Data, Settings, Scene, Audio, Pool)에 둔다.
- **Logic은 App.Data만 읽을 수 있다.** App.Settings / Scene / Audio / Pool은 View만 쓴다.
- DDOL에는 `Runner` 하나만 올린다. `RuntimeInitializeOnLoadMethod(BeforeSceneLoad)`에서 생성한다. 씬은 Main, Game 2개뿐이며 Boot 씬은 없다.
- 서비스 클래스, ServiceLocator, `IGameSessionListener`를 만들지 않는다.

### 엔티티와 이벤트
- 엔티티는 객체 참조 대신 `EntityID { index, generation }`로 가리킨다. World는 종류별 배열 + 빈 슬롯 목록으로 보관하고, 슬롯 재사용 시 generation을 올린다.
- 이벤트는 **struct**이며 발생 시점의 값을 복사해 담는다. 객체 참조를 담지 않는다.
- Runner가 Update에서 이벤트 큐를 비우며 View에 배달한다. View는 `OnDisable`에서 구독을 해제한다.

### 입력
- 프로젝트는 새 Input System 전용이다. 구 `UnityEngine.Input` API를 쓰지 않는다.
- `wasPressedThisFrame` 등 프레임 단위 입력은 Update에서만 읽는다.
- Runner가 Update에서 `world.Input`에 쌓고, CommandSystem이 FixedTick에서 소비한다.

### 이동
- `NavMeshAgent`의 자동 이동을 쓰지 않는다. `NavMesh.CalculatePath`로 경로만 받고 이동은 HeroSystem이 처리한다.
- 적은 웨이포인트를 따라 이동한다.

### 전투 · 연출
- 데미지 판정 시점은 Logic의 타이머가 결정한다. 애니메이션 이벤트로 데미지를 넣지 않는다.
- 적 사망 시 Logic 슬롯은 즉시 비우고, 사망 연출은 View가 이벤트를 받아 재생한다.

### 카메라
- Cinemachine(3.x, 네임스페이스 `Unity.Cinemachine`)을 쓴다. 카메라는 View다.
- 추적 대상은 히어로 **View Transform**이다. Logic 위치를 추적 대상으로 쓰지 않는다.

### UI
- View-Presenter 구조. Presenter가 World를 읽고 이벤트를 받아 View를 갱신한다.
- View의 버튼 입력은 Presenter를 거쳐 `world.Input`에 명령으로 들어간다.

### 단위
- 시뮬레이션은 50Hz 고정 (1틱 = 20ms). 시간 값은 데이터와 로직 모두 **정수 틱**으로 다루고, 필드명은 `~Ticks`로 끝낸다. 초나 ms를 쓰지 않는다.
- 거리는 월드 유닛(1 = 1m), XZ 평면 기준. 거리 비교는 `sqrMagnitude`로 한다.

## 컨벤션 핵심 (상세는 `CONVENTIONS.md`)
- 네임스페이스는 `Project05.*`, 폴더 경로와 일치. 코드 본문에서 .NET 타입은 `using` 또는 `global::System.~`로 접근한다.
- 줄임말 금지. 허용 약어는 UI, HP, XP, ID뿐이며 항상 대문자 (`currentHP`, `entityID`).
- 상수는 `MAX_HP`처럼 UPPER_SNAKE_CASE.
- `var` 금지. 파일 하나에 타입 하나, 중첩 타입 금지.
- 멤버 배치: public 필드 → 직렬화 private 필드 → private 필드 → const.
- 배열 원소 struct는 반드시 `ref`로 접근한다.
- public 함수에는 한국어 XML 문서 주석, public 변수에는 선언 윗줄에 한 줄 주석.
- 커밋 메시지는 `CONVENTIONS.md` 6장의 type(`feat`, `fix`, `refactor`, `chore`, `docs`, `balance`, `asset`)을 범위에 맞게 쓴다.

## WebGL 주의
- `Instantiate` / `Destroy`를 게임 루프에서 반복하지 않는다. View 오브젝트는 `App.Pool`을 쓴다.
- 멀티스레드(`System.Threading`, `Task.Run`)를 쓰지 않는다.
- 매 틱 할당(LINQ, 클로저, 박싱, 문자열 결합)을 피한다.

## 작업 방식
- 설계에 없는 시스템이나 규칙을 새로 도입해야 하면 코드 작성 전에 먼저 물어본다.
- 수치는 하드코딩하지 않고 `App.Data`(ScriptableObject)에서 읽는다.
- `DESIGN.md`의 미결정 사항에 해당하는 부분은 임의로 정하지 말고 확인한다.
