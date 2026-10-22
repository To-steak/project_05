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
7. **스테이지는 무한**이다. 승리는 없고, 코어 HP 또는 히어로 목숨이 0이 되면 패배한다. 도달한 웨이브가 기록이 된다.

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
| 지상 | 자동 | 근거리 광역 / 원거리 자동 공격 |
| 지상 · 빈 공터 근처 | Space | 타워 건설 |
| 지상 · 타워 근처 | Space | 탑승 |
| 지상 · 코어 근처 | Space | 코어 UI 열기 (타워 해금, 타워 업그레이드) |
| 탑승 중 | 좌클릭 | 클릭 지점/적 방향으로 사격 |
| 탑승 중 | Space | 하차 |
| 레벨업 선택 중 | 1 / 2 / 3 (임시) | 선택지 고르기 |

- **Space는 상황에 따른 상호작용 키**다. 대상(빈 공터 / 타워 / 코어)의 상호작용 범위 안에 들어가면 그 대상 위에 **Space 아이콘**을 표시한다 (View).
- 좌클릭은 이동(지상)과 사격(탑승 중)에만 쓴다.
- 상호작용 대상이 겹치지 않도록 레벨 디자인으로 보장한다 (3.2 공터 배치).

### 카메라
- **Cinemachine으로 히어로를 따라가는 탑다운 카메라.** 플레이어가 직접 카메라를 조작하지 않는다.
- 카메라는 View다. Cinemachine은 LateUpdate에서 갱신되므로 Logic/View 규칙과 맞는다.
- **추적 대상은 히어로 View의 Transform(보간된 위치)**이다. Logic 위치를 직접 따라가면 50Hz 계단 움직임 때문에 화면이 떨린다.
- 데드존과 감쇠(damping)를 둬서 작은 이동에는 카메라가 흔들리지 않게 한다.
- **실행 순서:** Runner의 ViewSystem(히어로 View 보간)이 CinemachineBrain보다 먼저 LateUpdate를 돌아야 카메라가 이번 프레임 위치를 따라간다.
  - 확인 결과 CinemachineBrain의 Script Execution Order는 **100** (Default Time 이후). Runner는 기본 순서(0)로 두면 Brain보다 먼저 실행된다.
  - Runner에 순서를 지정해야 할 일이 생기면 **100보다 작은 값**으로 한다.
  - CinemachineBrain의 Update Method는 **Late Update**로 고정한다.
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
| 2 | x 이내에 적이 없고, 반경 `rangedRange`(R) 이내에 적이 있음 | 원거리 | R 이내의 최근접 적 방향으로 직선 투사체 1발 |
| - | R 이내에도 적이 없음 | 공격 안 함 | 타이머는 준비 상태로 대기 |

  - `rangedRange`(R)는 데이터 값이며, **화면에 보이는 영역 안에 R 원이 항상 들어오도록** 카메라를 설정한다 (카메라 가시 영역 > R).
    - 따라서 원거리는 화면 안의 적만 노린다. Logic은 카메라를 참조하지 않고 R만으로 판정한다.
  - 거리 판정은 XZ 평면 기준, `sqrMagnitude`로 비교 (`거리² ≤ 사거리²`). "이내"는 **이하(≤)**.
  - 공격 후 타이머를 `attackIntervalTicks`로 재설정.
  - **원거리는 Logic 직선 투사체 (확정).** 발사 시 최근접 적의 위치 방향(XZ)으로 `projectileSpeed`(유닛/틱)씩 직진한다.
    - 매 틱 이동 구간에 처음 닿는 적 1명에게 피해를 주고 사라진다. 닿음 = 적 중심과 이동 구간의 최단 거리 ≤ 적 반경 + `projectileRadius`.
    - 대상이 움직이면 빗나갈 수 있다. 최대 비행 거리 R을 다 쓰면 피해 없이 사라진다.
    - 생성은 다른 엔티티처럼 SpawnSystem이 하므로 발사한 다음 틱부터 날아간다.
  - **탑승 중에는 히어로 자동 공격이 꺼진다.**
- 체력: `currentHP` / `maxHP`.
- **피해: 적과의 접촉 피해.** 히어로와 적의 거리가 접촉 반경(히어로 반경 + 적 반경) 이하이면 적의 `contactDamage`만큼 피해를 받는다. 적은 멈추지 않고 지나간다.
- **목숨: 3개.** HP가 0이 되면 목숨 1개를 잃고, `respawnTicks` 후 부활 지점에서 HP 가득 찬 상태로 부활한다.
- **피격 무적:** 피해를 받으면 `invincibilityTicks` 동안 추가 피해를 받지 않는다. 무적 중 깜빡임은 View가 표현한다.
- **탑승 중 무적:** 탑승 중에는 접촉 피해를 받지 않는다.
- **목숨이 0이 되면 즉시 패배.**
- 드롭 획득: 획득 반경(자석 반경) 안의 드롭을 자동 수집.

### 3.2 타워
- **공터(설치 자리):** 맵에 정해진 자리만 허용한다. **현재 4개**이므로 타워는 최대 4개.
  - 레벨 디자인: 코어를 중심으로 같은 반지름, 같은 각도 간격(90°)에 배치한다. 공터 사이 거리와 공터-코어 거리를 상호작용 반경의 2배보다 크게 두어 Space 대상이 겹치지 않게 한다.
- **타워 종류:** 여러 종류가 있다 (종류는 미정).
- **해금:** 코어 근처에서 Space → 코어 UI에서 골드를 지불해 타워 종류를 해금한다.
- **건설:** 빈 공터 근처에서 Space를 누르면 **골드를 지불**하고 타워를 건설한다.
- **업그레이드:** 코어 근처에서 Space → 코어 UI에서 **4개 타워를 각각** 업그레이드한다.
  - 업그레이드하면 **최대 HP와 공격력이 모두 증가**하고, **현재 HP는 최대로 회복**한다.
- **코어 UI가 열려 있는 동안 게임을 일시정지한다** (레벨업 선택과 같은 방식).
- **타워 HP:** 타워는 `currentHP` / `maxHP`를 가진다.
  - **적의 우선 공격:** 경로를 따라가던 적이 타워의 `aggroRange` 안에 들어오면 멈추고 타워를 공격한다. 코어 포위와 같은 방식으로 타워 주변 자리에 배정된다. 적은 경로를 벗어나 타워를 찾아가지 않는다.
  - 타워가 부서지면 그 타워를 공격하던 적은 경로 추종으로 돌아간다.
  - **파괴:** HP가 0이 되면 타워가 사라지고 **공터가 빈다.** 다시 건설해야 하며 **업그레이드도 초기화**된다. (타워 종류 해금은 유지)
  - **탑승 중 파괴:** 히어로는 강제 하차한다.
  - **수리:** 코어 UI에서 골드를 지불해 타워 HP를 회복한다.
- **미탑승:** 사거리 내 적을 자동 공격한다.
- **탑승:** 건설된 타워 근처에서 Space로 탑승한다.
  - **타워 공격력 = 히어로 공격력** (레벨업으로 올린 공격력이 그대로 적용된다)
  - 자동 공격이 꺼지고, **좌클릭 시 타워가 사격**한다
  - 제안: 타워의 공격 타이머가 준비된 상태에서만 클릭이 사격으로 처리되고, 준비 전 클릭은 무시
  - 히어로는 타워 위치에 고정, 드롭 획득 불가
- 탑승 판정: 히어로와 타워 거리 ≤ 탑승 반경.

### 3.3 적
- 웨이포인트 경로를 따라 이동 (NavMesh 사용 안 함).
- **코어 도달 시 사라지지 않는다.** 코어 주변에 멈춰 코어를 둘러싸고, `coreAttackIntervalTicks`마다 `coreDamage`만큼 코어를 때린다.
  - 둘러싸는 위치: 코어 중심 원 위의 자리(슬롯)를 도착 순서대로 배정한다. 자리가 차면 바깥 원에 배정한다. (물리 충돌 없이 결정적으로 배치)
  - **자리 배치 (확정):** 판 시작 시 동심원 위에 자리를 미리 계산한다. 고리 r의 반지름 = `siegeFirstRingRadius + siegeRingSpacing × r`, 자리 수 = `max(1, ⌊둘레 ÷ siegeSlotSpacing⌋)`. 자리 총수는 적 저장소 용량과 같다. 값은 GameDefinition.
  - **배정 (확정):** 경로 끝에 닿은 적은 빈 자리가 있는 가장 안쪽 고리에서 자기 위치에 가장 가까운 자리를 받아 직선으로 걸어간다. 도착하면 공격 주기 뒤부터 코어를 때린다.
  - **재사용 (확정):** 적이 죽으면 자리가 비고, 이후 도착하는 적이 다시 쓴다.
  - 코어를 때리는 적도 처치해야 진행도가 오른다. 히어로가 코어 주변 적 무리에 뛰어들면 접촉 피해 위험이 있다.
- 사망 시 골드·경험치 드롭.

### 3.4 웨이브
- 웨이브 데이터: 적 종류, 수, 스폰 간격, 스폰 갈래.
- 웨이브 사이 준비 시간 (값 TBD).
- **웨이브 이름: `스테이지-웨이브` 형식.** 1-1, 1-2, 1-3, …, 5-1, 5-2.
- **웨이브마다 적 수가 정해져 있다.** 진행도는 `current`(처치 수) / `max`(이번 웨이브 적 수)로 표시한다.
- **스테이지는 무한.** 스테이지마다 웨이브 10개 (n-1 ~ n-10).
- **n-10은 보스 웨이브.** 처치 수가 `max`에 도달하면 보스가 등장하고, 보스를 처치하면 스테이지 클리어 → 다음 스테이지 n+1-1.
- 일반 웨이브(n-1 ~ n-9)는 처치 수가 `max`에 도달하면 클리어.
- **난이도 증가:** 웨이브가 오를수록 적의 HP, 드롭 골드, 드롭 XP가 수식으로 증가한다. 수식은 3.4.1.

#### 3.4.1 증가 수식 (방향 확정: HP 지수, 보상 선형. 계수는 밸런싱 시 결정)
누적 웨이브 번호 `w = (n - 1) × 10 + x` (1-1 → 1, 1-10 → 10, 2-1 → 11).

| 값 | 수식 | 성격 |
|---|---|---|
| 적 HP | `round(baseHP × hpGrowth^(w - 1))` | 지수 증가. 언젠가 반드시 막히게 만든다 |
| 드롭 골드 | `round(baseGold × (1 + goldGrowth × (w - 1)))` | 선형 증가. 경제가 폭주하지 않게 한다 |
| 드롭 XP | `round(baseXP × (1 + xpGrowth × (w - 1)))` | 선형 증가 |
| 보스 | 위 값 × `bossHPMultiplier`, `bossGoldMultiplier`, `bossXPMultiplier` | |

- 예시: `hpGrowth = 1.08`이면 w = 10에서 약 2배, w = 30에서 약 9배, w = 50에서 약 43배.
- 계수는 `~Definition`에 두고, 계산은 적 **스폰 시 1회** 하여 엔티티에 복사한다 (접근 깊이 규칙).
- 웨이브당 적 수 **`max = x × 10`** (1-1 → 10, 1-10 → 100, 2-1 → 10). **임시 규칙**이며 밸런싱 때 수정한다.
- 웨이브별 적 종류 구성: 무한이므로 생성 규칙으로 정한다 (적 종류 해금 조건, 등장 비율). **세부 규칙은 나중에 정한다.**

#### 3.4.2 점수
- World에 `score` 필드를 둔다. 결과 화면에 도달 웨이브와 함께 표시한다.
- **임시 점수 규칙:** 일반 적 처치 1점, 보스 처치 10점. 밸런싱 때 바꾼다. 점수 값은 적 종류별 `EnemyDefinition.killScore`에 두고 스폰 시 적에 복사한다 (확정).
- 최고 도달 웨이브는 누적 웨이브 번호 `w = (n - 1) × 10 + x` 하나로 저장·비교하고, 표시할 때만 `n-x`로 바꾼다.
- **최고 점수와 최고 도달 웨이브를 PlayerPrefs에 저장하고 Main 화면에 표시한다.**
  - 저장은 게임 단계가 패배로 바뀐 것을 Core가 감지했을 때 World 값을 읽어 기존 기록과 비교해 갱신한다. System은 저장에 관여하지 않는다.
- 점수 규칙은 임시이며 밸런싱 때 확정한다.

### 3.5 코어
- HP 보유.

### 3.5.1 패배 조건 (둘 중 하나라도 만족하면 즉시 패배)
1. 코어 HP가 0이 된다.
2. 히어로의 목숨이 0이 된다.

### 3.5.2 패배 이후
- 게임 단계가 Defeat가 되면 **모든 System 실행을 멈춘다.** 마지막 화면은 그대로 유지한다 (ViewSystem은 계속 돌며 멈춘 World를 표시).
- **결과 패널:** 점수 텍스트, 버튼 두 개.
  - **재시작:** Game 씬을 다시 로드해 새 판을 시작한다.
  - **메인으로:** Main 씬을 로드한다.
- 버튼은 UI 이벤트 예외(onClick)로 처리하며, World가 아닌 App.Scene을 호출한다.
- 패배 판정은 GameFlowSystem이 틱 시작에 한다. 지난 틱에 코어 HP나 목숨이 0이 됐으면 이번 틱에 Defeat가 되고, 그다음 틱부터 Runner가 System을 실행하지 않는다.

### 3.6 경제 · 성장

| 자원 | 획득 | 용도 |
|---|---|---|
| 골드 | 적 드롭 | 타워 건설·강화 |
| 경험치 | 적 드롭 | 히어로 레벨업 |

- 레벨업 시 3개 선택지 중 1개 선택 (히어로 강화 / 탑승 시 타워 강화 / 특수 능력).
- **레벨업 선택지 (초안):** 아래 목록에서 3개를 시드 고정 랜덤으로 뽑아 제시한다 (한 번의 제시 안에서 중복 없음).

| 선택지 | 효과 대상 |
|---|---|
| 캐릭터 HP | 히어로 `maxHP` |
| 코어 HP | 코어 `maxHP` |
| 캐릭터 공격력 | 히어로 공격력 |
| 골드 획득량 | 드롭 골드 획득 배율 |
| 경험치 획득량 | 드롭 XP 획득 배율 |

  - **증가 방식:** 캐릭터 HP, 코어 HP, 캐릭터 공격력은 고정값. 골드·경험치 획득량은 정수 퍼센트 (`goldGainPercent = 110` → 1.1배).
  - **HP 선택 시:** 최대 HP를 올리고 현재 HP를 **전부 회복**한다 (캐릭터 HP, 코어 HP 모두).
  - 같은 선택지 반복 선택의 상한은 지금 고려하지 않는다.
- **레벨업 선택 중 일시정지 (확정).** 선택지가 열려 있는 동안 게임 진행이 멈추고, 플레이어는 강화할 내용을 고른다.
  - `Time.timeScale`을 쓰지 않는다 (0이면 FixedUpdate 자체가 멈춰 선택 명령도 처리할 수 없다). World의 일시정지 상태로 처리한다.
  - 일시정지 중에는 **CommandSystem만 실행**하고, 열려 있는 UI의 명령(레벨업 선택 / 코어 UI의 해금·업그레이드·닫기) 외의 명령은 버린다. 나머지 System은 실행하지 않는다.
  - 코어 UI도 같은 일시정지 상태를 쓴다.
  - 한 번에 여러 레벨이 오르면 대기 중인 레벨업 수만큼 선택을 연속으로 진행한다.

### 3.7 게임 흐름

씬은 **Main, Game 2개**.

```
[Main] 시작 버튼 → 로딩 패널 (Game 씬 비동기 로드)
       → 로드 완료 시 "시작" 버튼 활성화 → 클릭 → Game 씬 활성화
[Game] 준비 ↔ 웨이브 진행 → 결과 (패배, 도달 웨이브 표시)
       → 재시작 (Game 재로드) 또는 메인으로 (Main 로드)
```

| 씬 | 구성 |
|---|---|
| Main | 게임 시작 버튼, 설정 버튼, 로딩 패널, 최고 점수·최고 도달 웨이브 |
| Game | 게임 플레이, 결과 패널 (점수, 재시작, 메인으로) |

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
| 웨이브 | 무한 (스테이지당 10, n-10 보스) |
| 레벨업 선택지 | 10~15개 |

### 적 초안
| 이름 | 특징 |
|---|---|
| 일반 | 기준 |
| 빠름 | 이동속도↑ HP↓ |
| 탱커 | HP↑ 이동속도↓ |
| 실드/특수 | TBD |
| 보스 | 매 n-10 |

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
3. Logic은 View를 참조하지 않는다. 그 순간의 일은 World의 **프레임 기록 목록**에 남긴다.
4. View는 World를 읽기만 한다. World를 직접 바꾸지 않는다 (입력은 입력 버퍼 경유).
5. **이벤트를 쓰지 않는다** (5.7).
6. **Transform은 View만 쓴다.** Logic은 위치를 데이터로 소유한다.

### 5.2 두 계층: World와 App

기준: **"한 판이 끝나면 버려도 되는가?"**

| 계층 | 수명 | 내용 |
|---|---|---|
| **World** | 한 판 (판마다 `new World()`) | 히어로, 적, 타워, 드롭, 골드, 경험치, 웨이브 진행, 코어 HP, 입력 버퍼, 프레임 기록 목록 |
| **App** | 프로그램 전체 (1회 생성) | 데이터 테이블, 설정, 기록, 씬 로더, 오디오, View용 GameObject 풀 |

- App과 그 기능 클래스는 **Foundation 어셈블리**에 둔다. Core에 두면 View·UI와 순환 참조가 된다 (CONVENTIONS 1.1). Bootstrap(Core)이 App을 채운다.

```csharp
public static class App {
    public static GameData Data;          // 베이킹된 struct 테이블, 읽기 전용
    public static SettingsStore Settings; // 설정 PlayerPrefs 저장/로드
    public static RecordStore Records;    // 최고 점수·최고 도달 웨이브 PlayerPrefs 저장/로드
    public static SceneLoader Scene;      // 비동기 씬 로드
    public static AudioPlayer Audio;      // AudioSource 관리
    public static ViewPool Pool;          // View GameObject 풀 (UnityEngine.Pool.ObjectPool 기반)
}
```

#### 접근 규칙
| 쪽 | World | App |
|---|---|---|
| Logic (System) | 읽기/쓰기 | **사용 금지** (World만 본다) |
| View | 읽기 | 사용 |

- 판 시작 시 World가 `App.Data`의 테이블 참조를 받아둔다 (`world.enemyStats = App.Data.enemyStats`). 그래서 System은 App을 몰라도 된다.

#### 베이킹 (확정)
Authoring의 원본 데이터를 Component의 struct로 변환한다. System은 Component만 안다.

| 시점 | 대상 | 결과 저장 위치 |
|---|---|---|
| 게임 시작 시 1회 (Core 부트스트랩) | ScriptableObject 정의 (`~Definition`) | `App.Data` (struct 테이블) |
| 판 시작 시 (Game 씬 활성화) | 씬의 Authoring 컴포넌트 (타워 슬롯, 웨이포인트, 스폰 지점 등) | `World` |

- 하나의 Definition은 로직용 struct(`EnemyStats`)와 View용 테이블(프리팹, VFX)로 나뉜다.
- 베이킹 결과는 복사본이므로 Play 중 실수로 값을 바꿔도 원본 에셋이 바뀌지 않는다.

#### 접근 깊이 규칙 (체이닝 방지)
- **테이블 조회는 생성 시점에만 한다** (스폰, 건설, 강화). 필요한 값은 엔티티 struct에 복사해 둔다.
- 매 틱 로직은 **엔티티 자신의 값만** 쓴다. 예: `enemy.moveSpeed`, `hero.attackIntervalTicks`.
- 그 외의 긴 접근은 함수 위쪽에서 지역 `ref`로 한 번 받아 둔다.
- 개별 강화(레벨업으로 특정 타워 강화 등)는 엔티티 값을 바꾸는 것으로 처리한다.

### 5.3 부트스트랩과 DDOL Runner
- `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]`에서 1회:
  1. `Runner` GameObject 생성, `DontDestroyOnLoad`.
  2. App 채우기. AudioSource 등 Unity 컴포넌트가 필요한 것은 Runner 오브젝트에 붙여서 넘긴다.
- **DDOL에 올라가는 것은 Runner 하나뿐.** Runner가 App 기능과 현재 World를 보관한다.
- 에디터에서 Game 씬에서 바로 Play해도 동작한다.
- 도메인 리로드를 끈 Play 대비: `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]`에서 App 등 static 값을 초기화한다.

#### Runner의 역할
| 시점 | 하는 일 |
|---|---|
| FixedUpdate | World가 있으면 System들을 5.5 순서로 호출 |
| Update | 입력을 수집해 `world.Input`에 적재 |
| LateUpdate | ① `~ViewSystem.Present(world, viewContext)`를 정해진 순서로 호출 (동기화, 보간, 기록 연출) ② Presenter 호출 ③ 프레임 기록 목록 비우기 |

### 5.4 System
- `~System` 이름의 **static 클래스**. **static 필드에 상태를 두지 않는다.**
- 시그니처: `public static void Tick(World world)`.
- System 간 호출: **직접 호출**한다 (예: `EnemySystem`이 적 사망 처리 중 `DropSystem.Spawn(world, ...)` 호출). 같은 틱 안에서 순서가 명확하다.
- Unity 패키지 `UnityEngine.InputSystem`과 이름이 겹치지 않도록 입력 처리 System은 **`CommandSystem`**으로 부른다.

### 5.5 System 목록과 Tick 순서

| 순서 | System | 역할 |
|---|---|---|
| 1 | CommandSystem | 입력 버퍼를 꺼내 이동·탑승·사격·건설 명령으로 해석 (건설은 비용 지불 후 생성 요청) |
| 2 | GameFlowSystem | 준비 / 웨이브 / 패배 상태 전이. **패배 판정을 여기서 한 곳에 모아 한다** (코어 HP, 히어로 목숨) |
| 3 | WaveSystem | 웨이브 스케줄, 적·보스 생성 요청 |
| 4 | HeroSystem | 이동, 탑승/하차, 자동 공격, 사망/부활 |
| 5 | TowerSystem | 미탑승 자동 공격, 탑승 사격 |
| 5.5 | ProjectileSystem | 직선 투사체 이동, 명중 판정(첫 적 1명에게 피해 후 소멸), 비행 거리 소진 시 소멸 (히어로·타워 공용) |
| 6 | EnemySystem | 이동, 사망 처리(드롭 생성 요청), 타워·코어 포위 자리 배정, 타워 공격, 코어 공격, 타워 파괴 처리(탑승 중이면 강제 하차) |
| 7 | DropSystem | 드롭 획득 판정 |
| 8 | CoreSystem | 코어 피해 처리 |
| 9 | **SpawnSystem** | 생성 요청 큐를 처리해 적, 보스, 투사체, 드롭, 타워를 실제로 생성 |

#### 생성 규칙
- **엔티티 생성은 SpawnSystem에서만 한다.** 다른 System은 World의 **생성 요청 큐**에 요청을 넣기만 한다.
- 생성 요청 큐는 종류별로 둔다: `Queue<EnemySpawnRequest>`, `Queue<ProjectileSpawnRequest>`, `Queue<DropSpawnRequest>`, `Queue<TowerBuildRequest>`.
- SpawnSystem은 틱의 마지막에 실행되므로, **이번 틱에 요청된 엔티티는 다음 틱부터 동작한다.** 순회 중 생성되는 예외 상황이 구조적으로 생기지 않는다.
- 테이블 조회, 난이도 증가 수식 계산, 엔티티에 값 복사는 SpawnSystem에서 한다.
- 용량이 차서 생성에 실패한 요청은 큐에 남겨 다음 틱에 다시 시도한다.
- **소멸은 즉시** 한다 (`Destroy`). 이후 System은 `IsAlive` 확인으로 건너뛴다.
- 생성 요청 큐는 구독이 없는 World 데이터이므로 이벤트 금지 규칙에 해당하지 않는다.

Tick 없이 다른 System이 직접 호출하는 것:

| System | 역할 |
|---|---|
| EconomySystem | 골드 증감, 건설·강화 비용 지불 |
| ProgressSystem | 경험치 증가, 레벨업, 선택지 생성·적용 |
| TargetingSystem | 반경 내 최근접 적, 반경 내 모든 적 조회 (히어로·타워 공용) |
| SessionSystem | 베이킹된 데이터로 새 World 초기화 |

### 5.6 엔티티 ID
- 엔티티는 객체 참조 대신 **`EntityID { int index; int generation; }`**로 가리킨다.
- World는 종류별 엔티티를 **배열 + 빈 슬롯 목록**으로 보관한다. 슬롯을 재사용할 때 generation을 1 올린다.
- generation이 다르면 이미 죽은 엔티티다. 기록·타깃 참조가 재사용된 슬롯을 잘못 가리키는 문제를 막는다.
- View는 `EntityID → GameObject` 매핑으로 자기 오브젝트를 찾는다.
- 저장소는 제네릭 `EntityStore<T> where T : struct` 하나로 구현한다. 생성(`TryCreate`), 참조 조회(`ref T Get`), 생존 확인(`IsAlive`), 소멸(`Destroy`), 순회를 제공한다.
- 히어로와 코어는 하나뿐이므로 저장소 없이 World 필드로 둔다.

| 저장소 | 용량 | 근거 |
|---|---|---|
| 적 | 256 | 웨이브 최대 100 + 보스 + 코어 포위로 남은 적 |
| 드롭 | 256 | 바닥에 동시에 깔릴 양 |
| 타워 | 타워 슬롯 수 | 슬롯 하나에 타워 하나 |
| 투사체 | 128 | 동시에 날아가는 투사체 상한 |

- 용량이 차서 적 생성이 실패하면 그 스폰은 다음 틱으로 미룬다.

### 5.7 Logic → View 전달 (이벤트 사용 금지)
- **이벤트를 쓰지 않는다.** C# `event`, delegate 구독, 이벤트 버스, 이벤트 큐 모두 금지.
- **예외: UI.** UI는 World가 제어하는 구조가 아니므로, `Button.onClick` 등 UnityEvent와 UI Presenter가 발행하는 이벤트는 허용한다.
  - 단, UI 이벤트 핸들러는 World를 직접 바꾸지 않는다. Game 씬에서는 `world.Input`에 명령을 넣는 것까지만 한다.
- View는 **Runner가 정해진 순서로 호출하는 `~ViewSystem`**이 World를 읽어 표현한다. 구독이 없으므로 구독 해제 누락 버그도 없다.
- 전달 방식은 두 가지뿐이다.

| 종류 | 방식 | 예 |
|---|---|---|
| 계속 이어지는 상태 | ViewSystem·Presenter가 매 프레임 World를 읽는다 | 위치, HP 바, 골드, 웨이브 번호, 탑승 여부, 감속 상태, 레벨업 대기, 게임 단계 |
| 그 순간의 일 | Logic이 World의 **프레임 기록 목록**에 추가 → ViewSystem이 읽고 연출 → Runner가 마지막에 비움 | 피격, 사망, 발사, 건설 |

- **프레임 기록**은 종류별 `List<~Record>`이며 World에 있다. struct이고 발생 시점의 값을 복사해 담는다 (EntityID, 위치, 종류, 수치). 객체 참조를 담지 않는다.
- 연출 순서는 Runner의 ViewSystem 호출 순서로 정한다 (예: 피격 연출 → 사망 연출).
- **View 오브젝트 생성·반납은 동기화 방식:** 각 ViewSystem이 World의 살아있는 엔티티와 자기가 가진 View를 비교해, 새 EntityID면 풀에서 꺼내고 사라진 EntityID면 반납한다. 생성용 기록은 두지 않는다.

#### 프레임 기록 목록 (초안)
| 분류 | 기록 | 담을 값 | 연출 |
|---|---|---|---|
| 흐름 | WaveStartedRecord | 스테이지, 웨이브 | 시작 배너, BGM |
| 흐름 | WaveClearedRecord | 스테이지, 웨이브 | 클리어 연출 |
| 흐름 | BossSpawnedRecord | 보스 ID, 위치 | 보스 등장 경고 |
| 흐름 | StageClearedRecord | 스테이지 | 스테이지 클리어 연출 |
| 히어로 | HeroMeleeAttackRecord | 위치, 반경 | 베기 애니메이션, 범위 VFX |
| 히어로 | HeroRangedShotRecord | 시작 위치, 대상 ID, 대상 위치 | 발사 애니메이션, 투사체 연출 |
| 히어로 | HeroDamagedRecord | 위치, 피해량 | 피격 연출 |
| 히어로 | HeroDiedRecord | 위치, 남은 목숨 | 사망 연출 |
| 히어로 | HeroRespawnedRecord | 위치 | 부활 연출 |
| 타워 | TowerBuiltRecord | 타워 ID, 위치 | 건설 연출 |
| 타워 | TowerUpgradedRecord | 타워 ID, 단계 | 강화 연출 |
| 타워 | TowerShotRecord | 타워 ID, 대상 위치, 탑승 사격 여부 | 사격 VFX·SFX |
| 타워 | TowerDamagedRecord | 타워 ID, 위치, 피해량 | 피격 연출 |
| 타워 | TowerDestroyedRecord | 타워 ID, 위치 | 파괴 VFX |
| 타워 | TowerRepairedRecord | 타워 ID | 수리 연출 |
| 적 | EnemyDamagedRecord | 적 ID, 위치, 피해량 | 피격 플래시, 데미지 숫자 |
| 적 | EnemyDiedRecord | 적 ID, 종류, 위치 | 사망 VFX |
| 적 | EnemyCoreAttackRecord | 적 ID, 위치 | 코어 공격 애니메이션 |
| 적 | EnemyTowerAttackRecord | 적 ID, 타워 ID, 위치 | 타워 공격 애니메이션 |
| 드롭 | DropCollectedRecord | 종류, 양, 위치 | 획득 연출, "+10G" 표시 |
| 코어 | CoreDamagedRecord | 피해량 | 피격 연출, 화면 경고 |
| 명령 | CommandRejectedRecord | 명령 종류, 사유 | 안내 문구, 실패 효과음 |

- 상태로 읽는 것 (기록 없음): 엔티티 생성·소멸, 탑승/하차, 감속, 코어 포위 도착, 레벨업 선택 대기, 게임 종료.

#### ViewContext (확정)
- World가 "한 판의 게임 상태"라면, **ViewContext는 "한 판의 화면 상태"**다. 실행 중 World와 GameObject를 이어주는 인스턴스.
- 담는 것: EntityID → View 오브젝트 매핑(종류별), View 풀 참조, 씬의 표현용 참조(Cinemachine 카메라, 코어 View 등).
- ViewSystem은 **static 필드 없이** `Present(World world, ViewContext viewContext)`로 받은 것만 처리한다. Logic System과 같은 규칙이다.
- Runner가 보관한다. Game 씬 진입 시 새로 만들고(씬의 표현용 참조 수집 포함), 퇴장 시 버린다. 풀과 매핑 비우기가 이것으로 처리된다.
- Authoring과 다르다: Authoring은 베이킹용 원본(실행 중 불변), ViewContext는 실행 중 계속 바뀌는 상태.

### 5.8 세션 수명주기
- **View 풀:** Unity 내장 `UnityEngine.Pool.ObjectPool<T>`을 쓴다. **Game 씬 진입 시와 퇴장 시 모두 풀과 EntityID → View 매핑을 비운다.**
  - 풀링된 오브젝트는 Game 씬에 속하므로 씬과 함께 파괴된다. 퇴장 시 비우는 것은 이미 파괴됐거나 파괴될 오브젝트의 참조를 남기지 않기 위함이다.
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
- Logic 엔티티는 `previousPosition`, `currentPosition`을 보관.
- View는 Update에서 `alpha = (Time.time - Time.fixedTime) / Time.fixedDeltaTime`으로 보간해 Transform에 반영.

### 5.12 전투 · 연출 규칙
- 데미지 판정 시점은 Logic의 타이머가 결정한다. 애니메이션 이벤트로 데미지를 넣지 않는다.
- **원거리는 Logic 투사체 (확정, 3.1).** 피해는 투사체가 명중한 틱에 ProjectileSystem이 준다. 근거리 광역은 공격 틱에 즉시 판정.
- 적 사망 시 Logic 슬롯은 즉시 비우고, 사망 연출은 ViewSystem이 EnemyDiedRecord를 읽어 재생.
- 이동 애니메이션은 View가 Logic 속도 값을 읽어 결정.

### 5.13 View-Presenter (UI)
- Runner가 Presenter를 순서대로 호출하고, Presenter는 World를 읽어 View를 갱신한다. (Presenter 등록 방식은 구현 시 결정)
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
- [x] 구조 → World + static System + App(DDOL Runner), 프레임 기록 + ViewSystem 루프 (이벤트 금지), 엔티티 ID
- [x] UI 이벤트 → 예외로 허용 (UnityEvent, UI Presenter 발행 이벤트). World는 `world.Input` 경유로만 변경
- [x] 레벨업 선택 중 일시정지 → World 일시정지, CommandSystem만 실행
- [x] 히어로 공격 방식 → 근거리·원거리 둘 다, 거리 x로 자동 전환
- [x] 공격 타이머 1개, 근거리 광역, 탑승 중 자동 공격 꺼짐
- [x] 카메라 투영 → Perspective
- [ ] 카메라 높이·기울기·FOV, 화면 비율
- [x] 원거리는 화면 안 적만 → 카메라 가시 영역 > R 제약
- [ ] Cinemachine 패키지 추가 (Package Manager)
- [x] 투사체 히트스캔 여부 → Logic 직선 투사체 (ProjectileSystem, 이동 구간의 첫 적 1명 명중)
- [ ] 수치 전반 (HP, 데미지, 사거리, 비용, 탑승 배율, 부활 시간)
- [ ] 적 4번째 종류, 보스 패턴
- [ ] 난이도 증가 수식 계수 (밸런싱)
- [x] 웨이브당 적 수 → `x × 10` (임시)
- [ ] 적 구성 생성 규칙 (나중에)
- [x] 점수 계산 규칙 → 임시: 일반 적 1점, 보스 10점 (밸런싱 때 수정)
- [x] 최고 점수·최고 도달 웨이브 → PlayerPrefs 저장, Main 표시
- [x] 레벨업 증가 방식 → HP·공격력 고정값, 골드·XP 획득량 정수 퍼센트
- [x] 레벨업 선택 상한 → 지금은 고려하지 않음
- [x] 레벨업 HP 회복량 → 전부 회복
- [x] 공터 → 정해진 자리 4개, 코어 중심 균등 각도 배치
- [x] 건설 비용 → 골드, 업그레이드 → 타워별, 코어 UI 중 일시정지
- [ ] 타워 종류 목록, 건설 시 종류 선택 방법
- [x] 타워 HP → 경로 근처 적이 우선 공격, 파괴 시 공터 비움·업그레이드 초기화, 탑승 중 강제 하차, 코어 UI 수리
- [x] 업그레이드 시 타워 최대 HP·공격력 증가, 현재 HP 전부 회복
- [ ] 아트/사운드 에셋 출처
