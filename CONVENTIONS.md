# 코드 컨벤션

> 이 문서의 규칙은 예외 없이 지킨다. 아키텍처 규칙은 `CLAUDE.md`, 설계는 `DESIGN.md`를 따른다.

---

## 1. 폴더 구조

`Project/Assets/Scripts/` 아래에 다음 상위 계층 폴더를 둔다.

| 폴더 | 내용 | 예시 |
|---|---|---|
| `Component/` | 순수 데이터. World, 엔티티 struct, 이벤트 struct, 데이터 테이블 struct | `World`, `Enemy`, `EnemyDiedEvent` |
| `Authoring/` | 에디터에서 작성하는 원본. ScriptableObject 정의, 씬 배치용 MonoBehaviour(타워 슬롯, 웨이포인트 등) | `EnemyDefinition`, `TowerSlotAuthoring` |
| `System/` | 상태 없는 static 로직 | `HeroSystem`, `CommandSystem` |
| `View/` | 공통 표현 관심사. 애니메이션, VFX, SFX, 카메라, 보간 | `EnemyView`, `HeroView` |
| `UI/View/` | UI 화면 표시 | `GoldView` |
| `UI/Presenter/` | UI View와 World를 연결 | `GoldPresenter` |
| `Core/` | DDOL Runner, 부트스트랩, App 기능 | `Runner`, `App` |

- **미결정:** Authoring → Component 변환(베이킹) 채택 여부. 채택하면 System은 Component만 참조한다.

### 1.1 Assembly Definition
폴더마다 Assembly Definition을 두어 참조 방향을 컴파일 단계에서 강제한다.

| 어셈블리 | 참조 가능 |
|---|---|
| Component | (없음) |
| System | Component (+ 베이킹 미채택 시 Authoring) |
| Authoring | Component |
| View | Component |
| UI | Component |
| Core | 전부 |

- System이 View나 UI를 참조하면 컴파일 에러가 나야 한다.

### 1.2 네임스페이스
- `Project05.*` 형식으로, 폴더 경로와 일치시킨다.

| 폴더 | 네임스페이스 |
|---|---|
| `Component/` | `Project05.Component` |
| `Authoring/` | `Project05.Authoring` |
| `System/` | `Project05.System` |
| `View/` | `Project05.View` |
| `UI/View/` | `Project05.UI.View` |
| `UI/Presenter/` | `Project05.UI.Presenter` |
| `Core/` | `Project05.Core` |

- **주의: `Project05.System` 때문에 `Project05.*` 네임스페이스 안에서 `System.`으로 시작하는 정규화된 이름은 .NET이 아니라 `Project05.System`으로 해석된다.**
  - .NET 네임스페이스는 파일 맨 위의 `using` 지시문으로 가져온다 (`using System;`, `using System.Collections.Generic;`). 파일 맨 위의 `using`은 이 문제의 영향을 받지 않는다.
  - 코드 본문에서 `System.Random`처럼 정규화된 이름을 써야 하면 `global::System.Random`으로 쓴다.

---

## 2. 이름 규칙

### 2.1 줄임말 금지
- 모든 식별자(타입, 필드, 프로퍼티, 메서드, 매개변수, 지역 변수, 파일, 폴더)에 줄임말을 쓰지 않는다.

| 금지 | 사용 |
|---|---|
| `SO` | `ScriptableObject` |
| `prevPosition`, `currPosition` | `previousPosition`, `currentPosition` |
| `cnt`, `idx`, `pos`, `dir` | `count`, `index`, `position`, `direction` |
| `mgr`, `btn`, `cfg` | `manager`, `button`, `configuration` |
| `i`, `j` (반복 변수 포함) | `index`, `enemyIndex` |

- 예외: Unity·.NET 등 외부 API의 이름(`Time.fixedDeltaTime`, `Vector3` 등)은 그대로 쓴다.

### 2.2 허용 약어
다음 약어만 허용하며, **위치와 관계없이 항상 모두 대문자**로 쓴다.

| 약어 | 올바름 | 틀림 |
|---|---|---|
| UI | `UIRoot`, `openUI` | `UiRoot`, `openUi` |
| HP | `HP`, `currentHP`, `MAX_HP` | `Hp`, `currentHp` |
| XP | `XP`, `requiredXP` | `Xp`, `requiredXp` |
| ID | `ID`, `entityID`, `EntityID` | `Id`, `entityId` |

### 2.3 대소문자
| 대상 | 규칙 | 예시 |
|---|---|---|
| 타입, 메서드, 프로퍼티 | PascalCase | `HeroSystem`, `Tick()` |
| public 필드 (Component struct) | camelCase | `currentPosition` |
| private 필드 | `_camelCase` | `_runner` |
| 지역 변수, 매개변수 | camelCase | `targetEnemy` |
| **상수 (`const`)** | **UPPER_SNAKE_CASE** | `MAX_HP`, `MAX_ENEMY_COUNT` |

### 2.4 접미사·접두사
| 대상 | 규칙 | 예시 |
|---|---|---|
| System | `~System` | `HeroSystem` |
| 이벤트 struct | `~Event` | `EnemyDiedEvent` |
| Authoring MonoBehaviour | `~Authoring` | `TowerSlotAuthoring` |
| ScriptableObject 정의 | `~Definition` | `EnemyDefinition` |
| View | `~View` | `EnemyView`, `GoldView` |
| Presenter | `~Presenter` | `GoldPresenter` |
| bool | `is`, `has`, `can` | `isBoarded` |
| 시간 값 (틱) | `~Ticks` | `attackIntervalTicks` |

---

## 3. 구조체와 참조

- Component의 엔티티 데이터는 **struct + public 필드**로 작성한다.
- 배열 원소 struct는 **반드시 `ref`로 접근**한다. 복사본을 수정하는 실수를 막는다.

```csharp
// 올바름
ref Enemy enemy = ref world.enemies[enemyIndex];
enemy.currentHP -= damage;

// 금지: 복사본을 수정하므로 원본이 바뀌지 않는다
Enemy enemyCopy = world.enemies[enemyIndex];
enemyCopy.currentHP -= damage;
```

- struct를 매개변수로 넘길 때 수정이 필요하면 `ref`, 읽기만 하면 `in`을 쓴다.
- 랜덤은 World가 보유한 시드 고정 `global::System.Random`을 쓴다. `UnityEngine.Random`은 System에서 쓰지 않는다.

---

## 4. 코드 스타일

- **`var` 사용 금지.** 모든 변수는 타입을 명시한다.
- **파일 하나에 타입 하나.** 파일명은 타입명과 같다.
- **중첩 타입(nested type) 금지.** enum, struct, class 모두 최상위에 별도 파일로 둔다.
- 중괄호는 다음 줄에 연다 (Allman).

### 4.1 멤버 배치 순서
타입 안에서 위에서부터 다음 순서로 배치한다.

1. public 필드
2. 직렬화 private 필드 (`[SerializeField] private`)
3. private 필드
4. 상수 (`const`)

- **제안 (확인 필요):** 메서드는 필드 아래에 public → private 순으로 둔다.

---

## 5. 주석

- 주석 의무는 **public 함수와 public 변수에만** 적용한다. private 멤버와 지역 변수는 필요할 때만 단다.

### 5.1 public 함수
- **한국어 XML 문서 주석**을 단다.
- `<summary>`는 필수. 매개변수가 있으면 `<param>`, 반환값이 있으면 `<returns>`도 단다.

### 5.2 public 변수
- 선언 **바로 윗줄에 한 줄 주석**을 단다.

```csharp
public struct Hero
{
    // 이전 틱의 위치 (View 보간용)
    public Vector3 previousPosition;
    // 현재 틱의 위치
    public Vector3 currentPosition;
    // 현재 체력
    public int currentHP;
    // 다음 공격까지 남은 틱
    public int attackCooldownTicks;
}
```

```csharp
public static class HeroSystem
{
    /// <summary>
    /// 히어로의 공격 타이머가 준비되면 상황에 맞는 공격을 실행한다.
    /// </summary>
    /// <param name="world">현재 판의 월드</param>
    public static void Tick(World world)
    {
        ref Hero hero = ref world.hero;
    }
}
```

---

## 6. 커밋 메시지

### 6.1 형식
```
<type>: <요약>

<본문 (선택)>
```

### 6.2 type과 범위
| type | 사용 범위 | 예시 |
|---|---|---|
| `feat` | 새 기능 추가. 게임 규칙, System, View, UI, 새 데이터 정의 | `feat: 히어로 근거리 광역 공격 추가` |
| `fix` | 의도와 다르게 동작하는 버그 수정 | `fix: 탑승 중 드롭이 획득되는 문제 수정` |
| `refactor` | 동작 변화 없는 코드 구조 변경. 이름 변경, 파일 이동, 함수 분리 | `refactor: EnemySystem 이동 로직 분리` |
| `chore` | 게임 코드와 무관한 작업. 패키지, 프로젝트 설정, 빌드 설정, .gitignore, .meta 정리 | `chore: Cinemachine 패키지 추가` |
| `docs` | 문서만 변경. DESIGN.md, CONVENTIONS.md, CLAUDE.md, README.md | `docs: 웨이브 설계 추가` |
| `balance` | 코드 변경 없는 수치·데이터 조정 (ScriptableObject 값, 웨이브 구성) | `balance: 탱커 HP 상향` |
| `asset` | 아트·사운드·프리팹·씬 등 에셋 추가 및 수정 | `asset: 적 사망 VFX 추가` |

- 한 커밋에는 한 type만 담는다. 섞이면 커밋을 나눈다.
- **확인 필요:** 요약·본문 언어 (한국어 / 영어).

---

## 7. 미결정
- [ ] Authoring → Component 베이킹 방식 채택 여부
- [ ] 메서드 배치 순서 (제안: 필드 아래 public → private)
- [ ] 커밋 메시지 언어
