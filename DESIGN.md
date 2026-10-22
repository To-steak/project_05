# 게임 설계 문서

> 가제: TBD · 버전: 0.1 (초안) · 작성일: 2026-10-22

---

## 0. 개요

| 항목 | 내용 |
|---|---|
| 장르 | 액션 타워 디펜스 |
| 엔진 | Unity 6 (6000.3.11f1), URP 17.3 |
| 입력 | Input System 1.19 (새 Input System 전용) |
| 길찾기 | AI Navigation 2.0 (NavMeshSurface) |
| 빌드 타겟 | WebGL |
| 시점 | 탑다운, 히어로 추적 카메라 (Cinemachine) |
| 개발 기간 | 24시간 (설계 8h · 로직 코드 5h · View/밸런싱/빌드 11h) |

**한 줄 소개:** 플레이어가 히어로를 직접 움직이며 싸우고, 번 골드로 타워를 세우고, 필요할 때 타워에 탑승해 직접 사격하며 기지를 지킨다.

---

## 1. 핵심 루프

1. 적이 여러 갈래 길을 따라 코어(기지)로 진격한다.
2. 히어로는 클릭으로 이동하며, 사거리 안의 적을 자동 공격한다.
3. 적이 죽으면 **골드**와 **경험치 구슬**을 떨어뜨린다. 히어로가 근처로 가야 획득된다.
4. 골드로 정해진 슬롯에 타워를 건설·강화한다.
5. 경험치로 히어로가 레벨업하고, 3개 선택지 중 1개를 고른다.
6. 위급한 갈래에서는 타워에 탑승해 강화된 화력으로 직접 사격한다.
7. 모든 웨이브를 막으면 승리, 코어 HP가 0이 되면 패배.

### 핵심 긴장 구조

| 선택 | 이득 | 대가 |
|---|---|---|
| 앞으로 나가 싸운다 | 드롭 획득, 빠른 처치 | 다른 갈래 방치 |
| 타워에 탑승한다 | 타워 화력 대폭 상승, 직접 조준 | 이동 불가, 드롭 획득 불가 |
| 골드를 타워에 쓴다 | 방어 안정성 | 강화/추가 건설 여력 감소 |

---

## 2. 조작

| 상태 | 입력 | 동작 |
|---|---|---|
| 지상 | 좌클릭 (바닥) | 클릭 지점으로 이동 (NavMesh 경로) |
| 지상 | 좌클릭 (빈 슬롯) | 건설 메뉴 열기 |
| 지상 | 좌클릭 (건설된 타워) | 강화 메뉴 열기 |
| 지상 | 자동 | 사거리 내 가장 가까운 적 자동 공격 |
| 지상 · 타워 근처 | Space | 탑승 |
| 탑승 중 | 좌클릭 | 클릭 지점/적 방향으로 사격 |
| 탑승 중 | Space | 하차 |
| 레벨업 선택 중 | 1 / 2 / 3 (임시) | 선택지 고르기 |

- 탑승 가능 범위 안에 들어가면 타워 위에 **Space 아이콘**을 표시한다 (View).

### 카메라
- **Cinemachine으로 히어로를 따라가는 탑다운 카메라.** 플레이어가 직접 카메라를 조작하지 않는다.
- 카메라는 View다. Cinemachine은 LateUpdate에서 갱신되므로 Logic/View 규칙과 맞는다.
- **추적 대상은 히어로 View의 Transform(보간된 위치)**이다. Logic 위치를 직접 따라가면 50Hz 계단 움직임 때문에 화면이 떨린다.
- 데드존과 감쇠(damping)를 둬서 작은 이동에는 카메라가 흔들리지 않게 한다.
- 제안: 탑승 시 타워용 가상 카메라로 전환 (우선순위 변경, 사거리가 보이도록 약간 줌아웃).
- **투영: Perspective**, 히어로를 내려다보도록 기울인 탑다운.
- **화면 밖 적 표시:** 화면 밖에서 코어(중심)로 다가오는 적은 화면 가장자리에 방향 화살표로 표시한다 (View).
- 미니맵은 WebGL 부담 때문에 후순위.

#### 카메라 ↔ 원거리 사거리 제약
- 원칙: **카메라가 비추는 지면 영역 > 히어로 원거리 사거리(R)**.
- Perspective + 기울기에서는 지면 가시 영역이 사다리꼴이라, 히어로에서 **가장 가까운 화면 가장자리까지의 지면 거리**가 R보다 커야 한다.
- 이 거리는 화면 비율에 따라 줄어든다 (세로로 좁은 창). 지원할 **최소 화면 비율 기준**으로 맞춘다.
- Cinemachine 데드존·감쇠로 히어로가 화면 중앙에서 벗어날 수 있으므로, 그 오프셋만큼 여유를 둔다.
- 제작 시 확인용으로 R 원을 Gizmo로 그려 Game 뷰에서 화면 안에 들어오는지 확인한다.
- 미결정: 카메라 높이·기울기·FOV 수치, 화면 비율.

---

## 3. 시스템 상세

### 3.1 히어로
- 클릭 이동: `NavMesh.CalculatePath`로 경로 코너를 받고, 로직이 FixedUpdate에서 코너를 따라 이동.
- 자동 공격: 근거리 / 원거리 두 종류가 거리에 따라 자동 전환된다.
  - 히어로는 **공격 타이머 하나**(`attackIntervalTicks`)만 가진다.
  - 타이머가 준비되면 그 틱의 상황에 따라 공격을 하나 고른다.

| 우선순위 | 조건 | 공격 | 대상 |
|---|---|---|---|
| 1 | 반경 `meleeRange`(x) 이내에 적이 있음 | 근거리 광역 베기 | x 이내의 **모든 적** |
| 2 | x 이내에 적이 없고, 반경 `rangedRange`(R) 이내에 적이 있음 | 원거리 | R 이내의 최근접 적 1명 |
| - | R 이내에도 적이 없음 | 공격 안 함 | 타이머는 준비 상태로 대기 |

  - `rangedRange`(R)는 데이터 값이며, **화면에 보이는 영역 안에 R 원이 항상 들어오도록** 카메라를 설정한다 (카메라 가시 영역 > R).
    - 따라서 원거리는 화면 안의 적만 노린다. Logic은 카메라를 참조하지 않고 R만으로 판정한다.
  - 거리 판정은 XZ 평면 기준, `sqrMagnitude`로 비교 (`거리² ≤ 사거리²`). "이내"는 **이하(≤)**.
  - 공격 후 타이머를 `attackIntervalTicks`로 재설정.
  - **탑승 중에는 히어로 자동 공격이 꺼진다.**
- 사망: 코어 위치에서 N초 후 부활. 사망은 게임오버가 아니다.
- 드롭 획득: 획득 반경(자석 반경) 안의 드롭을 자동 수집.

### 3.2 타워
- **설치 위치는 맵에 미리 정해진 슬롯만 허용.**
- 건설 → 강화 1단계 → 강화 2단계.
- 빈 타워(미탑승): 기본 성능으로 자동 공격.
- 탑승 시:
  - 공격력·공격속도 배율 상승 (값 TBD)
  - 히어로의 레벨업 스탯이 타워에 추가 적용
  - 자동 공격이 꺼지고, **좌클릭 시 타워가 사격**한다
  - 제안: 타워의 공격 타이머가 준비된 상태에서만 클릭이 사격으로 처리되고, 준비 전 클릭은 무시
  - 히어로는 타워 위치에 고정, 드롭 획득 불가
- 탑승 판정: 히어로와 타워 거리 ≤ 탑승 반경.

### 3.3 적
- 웨이포인트 경로를 따라 이동 (NavMesh 사용 안 함).
- 코어 도달 시 코어에 피해를 주고 소멸.
- 사망 시 골드·경험치 드롭.

### 3.4 웨이브
- 웨이브 데이터: 적 종류, 수, 스폰 간격, 스폰 갈래.
- 웨이브 사이 준비 시간 (값 TBD).

### 3.5 코어
- HP 보유. 0이 되면 패배.

### 3.6 경제 · 성장

| 자원 | 획득 | 용도 |
|---|---|---|
| 골드 | 적 드롭 | 타워 건설·강화 |
| 경험치 | 적 드롭 | 히어로 레벨업 |

- 레벨업 시 3개 선택지 중 1개 선택 (히어로 강화 / 탑승 시 타워 강화 / 특수 능력).
- **제안:** 선택하는 동안 세션 로직 틱 정지.

### 3.7 게임 흐름

씬은 **Main, Game 2개**.

```
[Main] 시작 버튼 → 로딩 패널 (Game 씬 비동기 로드)
       → 로드 완료 시 "시작" 버튼 활성화 → 클릭 → Game 씬 활성화
[Game] 준비 ↔ 웨이브 진행 → 결과 (승리/패배)
       → 재시작 (Game 재로드) 또는 메인으로 (Main 로드)
```

| 씬 | 구성 |
|---|---|
| Main | 게임 시작 버튼, 설정 버튼, 로딩 패널 |
| Game | 게임 플레이, 결과 패널 |

- **로딩 패널:** `LoadSceneAsync` + `allowSceneActivation = false`로 로드. 진행률이 0.9에 도달하면 로드 완료로 보고 시작 버튼을 활성화. 클릭 시 `allowSceneActivation = true`.
- 로딩 패널의 시작 버튼 클릭은 WebGL 오디오 잠금 해제용 사용자 입력도 겸한다.
- **설정:** BGM/SFX 볼륨 (항목 TBD). `PlayerPrefs`로 저장 (WebGL에서는 IndexedDB에 저장됨).

---

## 4. 콘텐츠 범위 (24h)

| 항목 | 수량 |
|---|---|
| 맵 | 1 (갈래 2~3개) |
| 히어로 | 1 |
| 타워 | 3~4종, 강화 2단계 |
| 적 | 4종 + 보스 1 |
| 웨이브 | 10 내외 |
| 레벨업 선택지 | 10~15개 |

### 적 초안
| 이름 | 특징 |
|---|---|
| 일반 | 기준 |
| 빠름 | 이동속도↑ HP↓ |
| 탱커 | HP↑ 이동속도↓ |
| 실드/특수 | TBD |
| 보스 | 마지막 웨이브 |

### 타워 초안
| 이름 | 특징 |
|---|---|
| 단일 공격 | 기준 |
| 범위 공격 | 느린 공속, 광역 |
| 감속 | 낮은 데미지, 이동속도 감소 |
| 고화력 | 긴 사거리, 느린 공속 (선택) |

---

## 5. 아키텍처

> ECS **패키지(DOTS)는 쓰지 않는다.** ECS의 철학(System은 함수, 상태는 데이터)만 순수 C#으로 가져온다.

### 5.1 원칙
1. **Logic / View 완전 분리.**
   - Logic: 게임 상태와 규칙. **FixedUpdate에서만 실행.**
   - View: 애니메이션, SFX, VFX, UI, 카메라. **Update / LateUpdate에서만 실행.**
2. **System은 상태를 갖지 않는다.** 한 판의 모든 상태는 **World**에 있다.
3. Logic은 View를 참조하지 않는다. 일어난 일은 **이벤트 큐**에 남긴다.
4. View는 World를 읽고 이벤트를 받는다. World를 직접 바꾸지 않는다 (입력은 입력 버퍼 경유).
5. **Transform은 View만 쓴다.** Logic은 위치를 데이터로 소유한다.

### 5.2 두 계층: World와 App

기준: **"한 판이 끝나면 버려도 되는가?"**

| 계층 | 수명 | 내용 |
|---|---|---|
| **World** | 한 판 (판마다 `new World()`) | 히어로, 적, 타워, 드롭, 골드, 경험치, 웨이브 진행, 코어 HP, 입력 버퍼, 이벤트 큐 |
| **App** | 프로그램 전체 (1회 생성) | 데이터 테이블, 설정, 씬 로더, 오디오, View용 GameObject 풀 |

```csharp
public static class App {
    public static GameData Data;          // ScriptableObject 테이블, 읽기 전용
    public static SettingsStore Settings; // PlayerPrefs 저장/로드
    public static SceneLoader Scene;      // 비동기 씬 로드
    public static AudioPlayer Audio;      // AudioSource 관리
    public static ViewPool Pool;          // View GameObject 풀
}
```

#### 접근 규칙
| 쪽 | World | App.Data | App.Settings / Scene / Audio / Pool |
|---|---|---|---|
| Logic (System) | 읽기/쓰기 | 읽기 | **사용 금지** |
| View | 읽기 | 읽기 | 사용 |

### 5.3 부트스트랩과 DDOL Runner
- `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]`에서 1회:
  1. `Runner` GameObject 생성, `DontDestroyOnLoad`.
  2. App 채우기. AudioSource 등 Unity 컴포넌트가 필요한 것은 Runner 오브젝트에 붙여서 넘긴다.
- **DDOL에 올라가는 것은 Runner 하나뿐.** Runner가 App 기능과 현재 World를 보관한다.
- 에디터에서 Game 씬에서 바로 Play해도 동작한다.
- 도메인 리로드를 끈 Play 대비: `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]`에서 App과 static 이벤트를 초기화한다.

#### Runner의 역할
| 시점 | 하는 일 |
|---|---|
| FixedUpdate | World가 있으면 System들을 5.5 순서로 호출 |
| Update | ① 입력을 수집해 `world.Input`에 적재 ② `world.Events` 큐를 비우며 View에 배달 |
| LateUpdate | (필요 시) View 마무리 |

### 5.4 System
- `~System` 이름의 **static 클래스**. **static 필드에 상태를 두지 않는다.**
- 시그니처: `public static void Tick(World world)`.
- System 간 호출: **직접 호출**한다 (예: `EnemySystem`이 적 사망 처리 중 `DropSystem.Spawn(world, ...)` 호출). 같은 틱 안에서 순서가 명확하다.
- Unity 패키지 `UnityEngine.InputSystem`과 이름이 겹치지 않도록 입력 처리 System은 **`CommandSystem`**으로 부른다.

### 5.5 System 목록과 Tick 순서

| 순서 | System | 역할 |
|---|---|---|
| 1 | CommandSystem | 입력 버퍼를 꺼내 이동·탑승·사격·건설 명령으로 해석 |
| 2 | GameFlowSystem | 준비 / 웨이브 / 승리 / 패배 상태 전이 |
| 3 | WaveSystem | 웨이브 스케줄, 적 스폰 |
| 4 | HeroSystem | 이동, 탑승/하차, 자동 공격, 사망/부활 |
| 5 | TowerSystem | 미탑승 자동 공격, 탑승 사격 |
| 6 | EnemySystem | 이동, 사망 처리, 코어 도달 처리 |
| 7 | DropSystem | 드롭 획득 판정 |
| 8 | CoreSystem | 패배 판정 |

Tick 없이 다른 System이 직접 호출하는 것:

| System | 역할 |
|---|---|
| EconomySystem | 골드 증감, 건설·강화 비용 지불 |
| ProgressSystem | 경험치 증가, 레벨업, 선택지 생성·적용 |

### 5.6 엔티티 ID
- 엔티티는 객체 참조 대신 **`EntityId { int index; int generation; }`**로 가리킨다.
- World는 종류별 엔티티를 **배열 + 빈 슬롯 목록**으로 보관한다. 슬롯을 재사용할 때 generation을 1 올린다.
- generation이 다르면 이미 죽은 엔티티다. 이벤트·타깃 참조가 재사용된 슬롯을 잘못 가리키는 문제를 막는다.
- View는 `EntityId → GameObject` 매핑으로 자기 오브젝트를 찾는다.

### 5.7 이벤트 큐 (Logic → View)
- Logic은 `world.Events`에 이벤트를 **추가만** 한다.
- Runner가 Update에서 큐를 비우며 View에 배달한다. 한 프레임에 FixedUpdate가 여러 번 돌아도 빠짐없이 전달된다.
- 이벤트는 **struct**이고, 발생 시점의 **값을 복사**해 담는다 (EntityId, 위치, 종류, 수치). 객체 참조를 담지 않는다.
- View는 구독 해제를 `OnDisable`에서 반드시 한다.

### 5.8 세션 수명주기
- Game 씬 활성화 → `world = new World()` → System 실행 시작.
- Game 씬을 떠남(재시작 포함) → `world = null`.
- 서비스별 초기화 코드가 필요 없다.

### 5.9 입력
- 프로젝트는 **새 Input System 전용**이다. 구 `UnityEngine.Input` API는 사용 금지 (실행 시 예외).
- `wasPressedThisFrame` 계열은 Update에서만 읽는다.
- Runner가 Update에서 입력을 수집해 `world.Input` 버퍼에 쌓는다. 클릭 지면 좌표 레이캐스트도 이때 한다.
- CommandSystem이 FixedTick에서 버퍼를 소비한다.
- 입력 액션은 `InputSystem_Actions.inputactions`를 기반으로 정의한다 (Click, Board 등).

### 5.10 이동
- 히어로: 이동 명령 시 `NavMesh.CalculatePath` → 코너 목록을 World에 저장 → HeroSystem이 따라감. `NavMeshAgent` 자동 이동 사용 안 함.
- 적: 웨이포인트 경로 추종.

### 5.11 View 보간
- Logic 엔티티는 `prevPosition`, `currPosition`을 보관.
- View는 Update에서 `alpha = (Time.time - Time.fixedTime) / Time.fixedDeltaTime`으로 보간해 Transform에 반영.

### 5.12 전투 · 연출 규칙
- 데미지 판정 시점은 Logic의 타이머가 결정한다. 애니메이션 이벤트로 데미지를 넣지 않는다.
- **제안:** Logic은 공격 즉시 판정(히트스캔), 투사체 비행은 View 연출로만 표현.
- 적 사망 시 Logic 슬롯은 즉시 비우고, 사망 연출은 View가 이벤트를 받아 별도로 재생.
- 이동 애니메이션은 View가 Logic 속도 값을 읽어 결정.

### 5.13 View-Presenter (UI)
- Presenter가 World를 읽고 이벤트를 받아 View를 갱신한다.
- View 입력(버튼 등) → Presenter → `world.Input` 버퍼에 명령 적재 (건설, 강화, 레벨업 선택).
- View는 World를 직접 바꾸지 않는다.

### 5.14 시간 설정
| 항목 | 값 |
|---|---|
| Fixed Timestep | 0.02 (50Hz) — **확정** |
| Maximum Allowed Timestep | 0.1 |

### 5.15 단위 규칙
| 값 | 단위 | 비고 |
|---|---|---|
| 거리, 사거리, 반경 | 월드 유닛 (1 = 1m) | XZ 평면 기준 |
| 시간, 주기, 쿨다운, 지속시간 | **틱** (1틱 = 20ms, 50틱 = 1초) | 데이터와 로직 모두 정수 틱 |
| 속도 | 유닛 / 틱 | |

- 시간 값은 데이터(ScriptableObject)에도 **틱 정수**로 저장한다. 필드명은 `~Ticks`로 끝낸다 (예: `attackIntervalTicks`).
- Fixed Timestep을 바꾸면 모든 틱 데이터를 다시 조정해야 하므로 **50Hz는 고정**한다.

---

## 6. WebGL 체크리스트
- [ ] 첫 3시간 안에 빈 씬 WebGL 빌드·배포 확인
- [ ] 압축 방식(Brotli/Gzip)과 서버 헤더 확인
- [ ] 오디오는 첫 사용자 클릭 이후 재생
- [ ] 적·드롭·VFX 전부 풀링
- [ ] 동시 적 수 상한 설정 (100~200)
- [ ] 탭 백그라운드 복귀 시 동작 확인

---

## 7. 미결정 사항
- [ ] 게임 제목, 테마/세계관
- [x] 구조 → World + static System + App(DDOL Runner), 이벤트 큐, 엔티티 ID
- [ ] 레벨업 선택 중 일시정지 여부
- [x] 히어로 공격 방식 → 근거리·원거리 둘 다, 거리 x로 자동 전환
- [x] 공격 타이머 1개, 근거리 광역, 탑승 중 자동 공격 꺼짐
- [x] 카메라 투영 → Perspective
- [ ] 카메라 높이·기울기·FOV, 화면 비율
- [x] 원거리는 화면 안 적만 → 카메라 가시 영역 > R 제약
- [ ] Cinemachine 패키지 추가 (Package Manager)
- [ ] 투사체 히트스캔 여부
- [ ] 수치 전반 (HP, 데미지, 사거리, 비용, 탑승 배율, 부활 시간)
- [ ] 적 4번째 종류, 보스 패턴
- [ ] 레벨업 선택지 목록
- [ ] 아트/사운드 에셋 출처
