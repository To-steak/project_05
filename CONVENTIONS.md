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

- **제안 (확인 필요):** View와 Runner/App이 들어갈 위치. 후보는 `View/`, `App/`을 상위 폴더로 추가하는 것.
- **제안 (확인 필요): Authoring → Component 변환(베이킹).** 게임 시작 시 ScriptableObject 데이터를 Component의 struct 테이블로 변환해 `App.Data`에 담는다. 그러면 System은 Component만 알면 되고 Authoring을 참조하지 않는다.

### 1.1 Assembly Definition
폴더마다 Assembly Definition을 두어 참조 방향을 컴파일 단계에서 강제한다.

| 어셈블리 | 참조 가능 |
|---|---|
| Component | (없음) |
| System | Component |
| Authoring | Component |
| View (제안) | Component |
| App (제안) | 전부 |

- System이 View나 Authoring을 참조하면 컴파일 에러가 나야 한다.

### 1.2 네임스페이스
- 폴더 경로와 일치시킨다. 예: `Game.Component`, `Game.System`, `Game.Authoring`.
- 주의: `Game.System` 네임스페이스 안에서는 .NET의 `System` 네임스페이스와 이름이 겹친다. .NET 타입은 `global::System.Random`처럼 `global::`로 접근한다.

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
- **확인 필요:** `Id`, `UI`, `Hp`, `Xp`처럼 널리 쓰이는 약어의 허용 여부.

### 2.2 대소문자
| 대상 | 규칙 | 예시 |
|---|---|---|
| 타입, 메서드, 프로퍼티, 상수 | PascalCase | `HeroSystem`, `Tick()`, `MaximumEnemyCount` |
| public 필드 (Component struct) | camelCase | `currentPosition` |
| private 필드 | `_camelCase` | `_runner` |
| 지역 변수, 매개변수 | camelCase | `targetEnemy` |

### 2.3 접미사·접두사
| 대상 | 규칙 | 예시 |
|---|---|---|
| System | `~System` | `HeroSystem` |
| 이벤트 struct | `~Event` | `EnemyDiedEvent` |
| Authoring MonoBehaviour | `~Authoring` | `TowerSlotAuthoring` |
| ScriptableObject 정의 | `~Definition` | `EnemyDefinition` |
| bool | `is`, `has`, `can` | `isBoarded` |
| 시간 값 (틱) | `~Ticks` | `attackIntervalTicks` |

---

## 3. 구조체와 참조

- Component의 엔티티 데이터는 **struct + public 필드**로 작성한다.
- 배열 원소 struct는 **반드시 `ref`로 접근**한다. 복사본을 수정하는 실수를 막는다.

```csharp
// 올바름
ref Enemy enemy = ref world.enemies[enemyIndex];
enemy.health -= damage;

// 금지: 복사본을 수정하므로 원본이 바뀌지 않는다
Enemy enemyCopy = world.enemies[enemyIndex];
enemyCopy.health -= damage;
```

- struct를 매개변수로 넘길 때 수정이 필요하면 `ref`, 읽기만 하면 `in`을 쓴다.
- 랜덤은 World가 보유한 시드 고정 `global::System.Random`을 쓴다. `UnityEngine.Random`은 Logic에서 쓰지 않는다.

---

## 4. 코드 스타일

- **`var` 사용 금지.** 모든 변수는 타입을 명시한다.
- **파일 하나에 타입 하나.** 파일명은 타입명과 같다.
- **중첩 타입(nested type) 금지.** enum, struct, class 모두 최상위에 별도 파일로 둔다.
- 중괄호는 다음 줄에 연다 (Allman).

---

## 5. 주석

### 5.1 함수
- **모든 함수**(public, private, 생성자, Unity 메시지 포함)에 **한국어 XML 문서 주석**을 단다.
- `<summary>`는 필수. 매개변수가 있으면 `<param>`, 반환값이 있으면 `<returns>`도 단다.

```csharp
/// <summary>
/// 히어로의 공격 타이머가 준비되면 상황에 맞는 공격을 실행한다.
/// </summary>
/// <param name="world">현재 판의 월드</param>
public static void Tick(World world)
{
    // 근거리 공격 사거리의 제곱값
    float meleeRangeSquared = world.hero.meleeRange * world.hero.meleeRange;
}
```

### 5.2 변수
- 변수 선언 **바로 윗줄에 한 줄 주석**을 단다.
- **확인 필요:** 적용 범위가 필드만인지, 지역 변수까지인지.

---

## 6. 미결정
- [ ] View, App(Runner) 폴더 위치
- [ ] Authoring → Component 베이킹 방식 채택 여부
- [ ] 허용 약어 목록 (`Id`, `UI`, `Hp`, `Xp` 등)
- [ ] 변수 주석 적용 범위 (필드 / 지역 변수)
- [ ] 커밋 메시지 언어와 접두사 규칙
