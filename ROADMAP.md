# 작업 로드맵

> 1단계는 순서대로 진행한다. 2단계는 레인별로 병렬 진행하므로 **새 세션에는 작업할 브랜치 이름을 지정**한다.
> 작업 전 `CLAUDE.md`, `DESIGN.md`, `CONVENTIONS.md`를 읽는다.
> 각 항목은 main 최신에서 브랜치를 따서 작업하고 PR을 연다. 병합은 사용자가 하며, **병합 후 이 문서의 체크박스를 갱신**한다.

---

## 0단계: 준비

- [x] 설계 문서 (DESIGN.md, CONVENTIONS.md, CLAUDE.md)
- [x] Scripts 폴더 구조와 폴더별 README.txt
- [x] `chore/webgl-deploy` (PR #1) — WebGL Decompression Fallback 활성화 (병합됨)
- [x] Actions 워크플로 제거 (로컬 빌드로 전환)
- [x] 빈 씬 WebGL 로컬 빌드 → itch.io 업로드 확인 (사용자 작업)

배포는 **로컬 빌드 후 itch.io에 직접 업로드**한다. GitHub Actions 자동 빌드는 쓰지 않는다.

---

## 1단계: 기반 (순차)

| 상태 | 브랜치 | 내용 | 확인 방법 |
|---|---|---|---|
| [x] | `chore/assembly-setup` | 폴더별 Assembly Definition과 참조 방향 (DESIGN 5장, CONVENTIONS 1.1) | System에서 View를 참조하면 컴파일 에러 |
| [ ] | `feat/core-loop` (PR #3) | Bootstrap, Runner, App, World, ViewContext, EntityID, EntityStore, SessionSystem. Fixed Timestep 0.02 / Maximum Allowed Timestep 0.1 | Game 씬 Play 시 Runner 생성, 틱 카운트 증가 (OnGUI 디버그) |
| [ ] | `feat/component-contract` | **데이터 계약 고정.** 폴더별 README.txt의 모든 Component(엔티티, 상태, 기록, 요청, 테이블, 입력) 정의, World 필드 전부 추가, 모든 System·ViewSystem의 빈 `Tick` / `Present` 껍데기, Runner의 틱 순서(DESIGN 5.5)와 ViewSystem 호출 순서 연결 | 컴파일 성공, Play 시 기존과 동일하게 동작 |

`feat/component-contract`가 병합되면 2단계 레인을 **병렬로** 진행할 수 있다. 레인은 동시에 2~3개까지 연다.

---

## 2단계: 레인별 병렬 작업

각 레인 안에서는 순서대로, 레인끼리는 병렬로 진행한다. **씬 파일 규칙은 CONVENTIONS 7.5를 따른다.**

### 레인 A: 히어로 (Game 씬 담당)

| 상태 | 브랜치 | 내용 | 확인 방법 |
|---|---|---|---|
| [ ] | `feat/hero-movement` | InputCollector, CommandSystem(이동), HeroSystem 이동, HeroViewSystem 보간, CameraViewSystem(Cinemachine 추적) | 클릭하면 히어로가 NavMesh 경로로 이동, 카메라가 따라옴 |
| [ ] | `feat/hero-combat` | TargetingSystem, 근거리 광역·원거리, 적 피해와 사망, 프레임 기록, 기본 연출 | 히어로가 적을 자동으로 처치 (레인 B 병합 후 확인) |
| [ ] | `feat/core-siege` | 코어 포위와 공격, 접촉 피해, 무적, 목숨과 부활, GameFlow(준비·웨이브·보스·스테이지·패배), 결과 패널 | **한 판이 처음부터 패배까지 돌아감** |

### 레인 B: 데이터와 적 (테스트 씬, 병합 후 Game 씬 배치)

| 상태 | 브랜치 | 내용 | 확인 방법 |
|---|---|---|---|
| [ ] | `feat/baking` | Hero·Enemy·WaveRule·Difficulty·Progression Definition, DefinitionBaker, SceneBaker(웨이포인트, 스폰, 코어, 공터, 히어로 시작점) | Play 시 World에 베이킹된 값이 들어감 (디버그 표시) |
| [ ] | `feat/enemy-wave` | WaveSystem(`x × 10`), SpawnSystem과 생성 요청 큐, EnemySystem 경로 이동, EnemyViewSystem 동기화와 풀 | 적이 웨이브마다 스폰돼 길을 따라 이동 |

### 레인 C: Main 씬 (Main 씬 담당, Game과 독립)

| 상태 | 브랜치 | 내용 | 확인 방법 |
|---|---|---|---|
| [ ] | `feat/main-scene` | Main 메뉴, 설정(SettingsStore), 로딩 패널(SceneLoader), 최고 기록(RecordStore) | Main → 로딩 → Game 진입, 설정 저장 유지 |

### 레인 D: 성장 로직 (System만, 화면 확인은 A·B 병합 후)

| 상태 | 브랜치 | 내용 | 확인 방법 |
|---|---|---|---|
| [ ] | `feat/drop-progress` | 드롭, 골드·XP 획득, 레벨업 일시정지와 선택지 UI | 레벨업 시 멈추고 선택 후 재개 |

UI는 레인 A·B가 끝날 때까지 OnGUI 디버그 패널로 대신한다 (결과 패널 제외).

---

## 3단계: 타워 (레인 A·B·D 병합 후)

| 상태 | 브랜치 | 내용 | 확인 방법 |
|---|---|---|---|
| [ ] | `feat/tower` | 공터 Space 건설, 자동 공격, 탑승과 클릭 사격, 타워 HP와 우선 공격, 파괴와 강제 하차 | 타워가 적을 막고 부서짐 |
| [ ] | `feat/core-menu` | 코어 UI: 해금, 타워별 업그레이드, 수리, 일시정지 | 코어에서 Space로 메뉴 사용 |

**`feat/tower` 시작 전 필요한 결정:** 타워 종류 목록, 건설 시 종류 선택 방법 (DESIGN 7장).

---

## 4단계: 마무리

- [ ] 화면 밖 적 화살표, Space 아이콘
- [ ] 연출 (VFX, SFX, 애니메이션)
- [ ] 카메라 수치와 원거리 사거리 제약 맞추기
- [ ] 밸런싱 (난이도 계수, 비용, 점수 규칙)
- [ ] 최종 WebGL 빌드와 itch.io 배포

---

## 순서를 이렇게 잡은 이유

- **데이터 계약을 먼저 고정하는 이유:** 모든 브랜치가 `World.cs`와 Runner의 틱 연결을 고치면 병렬 작업 시 매번 충돌한다. Component와 빈 System을 먼저 다 만들어 두면, 이후 브랜치는 각자 맡은 System 파일의 내용만 채운다.
- **UI는 최대한 뒤로:** 레인 A·B가 끝날 때까지 OnGUI 디버그 패널로 수치를 보면 충분하다.
- **타워를 뒤로 미룬 이유:** 타워 종류와 건설 시 종류 선택 방법이 미정이다. 2단계는 이 결정 없이 진행할 수 있다.
- **병렬의 병목은 리뷰다.** 리뷰어가 한 명이므로 레인은 동시에 2~3개까지만 연다.
- **PR 크기:** `feat/hero-combat`, `feat/core-siege`, `feat/tower`는 큰 편이라 리뷰가 길어지면 둘로 나눈다.

---

## 새 세션 시작 프롬프트 예시

```
To-steak/project_05 레포에서 작업한다.
CLAUDE.md, DESIGN.md, CONVENTIONS.md, ROADMAP.md를 먼저 읽어라.
ROADMAP.md에서 <브랜치 이름> 항목을 main 최신에서 브랜치를 따서 구현하고 PR을 열어라.
(병렬 레인을 쓰므로 어떤 브랜치를 할지 이름으로 지정한다.)
```
