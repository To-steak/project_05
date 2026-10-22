# 작업 로드맵

> 한 번에 브랜치 하나만 진행한다. 새 세션은 **체크되지 않은 첫 항목**을 다음 작업으로 본다.
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

## 1단계: 기반

| 상태 | 브랜치 | 내용 | 확인 방법 |
|---|---|---|---|
| [x] | `chore/assembly-setup` (PR #2) | 폴더별 Assembly Definition과 참조 방향 | System에서 View를 참조하면 컴파일 에러 |
| [x] | `feat/core-loop` (PR #3) | Bootstrap, Runner, App, World, ViewContext, EntityID, EntityStore, SessionSystem, 시간 설정 | Game 씬 Play 시 Runner 생성, 틱 카운트 증가 |
| [x] | `feat/component-contract` (PR #5) | 모든 Component, World 필드, System·ViewSystem 껍데기, Runner 연결 | 컴파일 성공 |
| [x] | `feat/baking` (PR #6) | Definition, DefinitionBaker, SceneBaker, 베이킹 테스트 씬 | Play 시 World에 베이킹된 값이 들어감 |

---

## 2단계: 기능 (한 번에 하나씩, 순서대로)

> 병렬 레인은 충돌이 잦아 중단했다. **한 번에 브랜치 하나만** 열고, 병합된 뒤 다음 항목을 시작한다.
> 닫힌 PR(#7, #8)의 브랜치는 원격에 남아 있다. 해당 항목을 다시 할 때 그 브랜치의 코드를 참고하거나 main 위로 다시 올려(rebase) 재사용한다.

| 상태 | 브랜치 | 내용 | 확인 방법 |
|---|---|---|---|
| [x] | `refactor/foundation` | App과 기반 기능(SettingsStore, RecordStore, SceneLoader, AudioPlayer, ViewPool)을 Core에서 `Foundation/` 어셈블리로 분리. View·UI → Foundation 참조, System은 참조 금지 (CONVENTIONS 1.1). 폴더별 README.txt도 이동 | View·UI에서 `App.~` 사용 시 컴파일 성공, System에서는 실패 |
| [x] | `feat/hero-movement` | InputCollector, CommandSystem(이동), HeroSystem 이동, HeroViewSystem 보간, CameraViewSystem(Cinemachine 추적). 닫힌 PR #7 참고 | 클릭하면 히어로가 NavMesh 경로로 이동, 카메라가 따라옴 |
| [ ] | `feat/enemy-wave` | WaveSystem(`x × 10`), SpawnSystem과 생성 요청 큐, EnemySystem 경로 이동, EnemyViewSystem 동기화와 풀 | 적이 웨이브마다 스폰돼 길을 따라 이동 |
| [ ] | `feat/hero-combat` | TargetingSystem, 근거리 광역·원거리, 적 피해와 사망, 프레임 기록, 기본 연출 | 히어로가 적을 자동으로 처치 |
| [ ] | `feat/core-siege` | 코어 포위와 공격, 접촉 피해, 무적, 목숨과 부활, GameFlow(준비·웨이브·보스·스테이지·패배), 결과 패널 | **한 판이 처음부터 패배까지 돌아감** |
| [ ] | `feat/drop-progress` | 드롭, 골드·XP 획득, 레벨업 일시정지와 선택지 UI. 닫힌 PR #8 참고 | 레벨업 시 멈추고 선택 후 재개 |
| [ ] | `feat/main-scene` | Main 메뉴, 설정(SettingsStore), 로딩 패널(SceneLoader), 최고 기록(RecordStore) | Main → 로딩 → Game 진입, 설정 저장 유지 |
| [ ] | `feat/tower` | 공터 Space 건설, 자동 공격, 탑승과 클릭 사격, 타워 HP와 우선 공격, 파괴와 강제 하차 | 타워가 적을 막고 부서짐 |
| [ ] | `feat/core-menu` | 코어 UI: 해금, 타워별 업그레이드, 수리, 일시정지 | 코어에서 Space로 메뉴 사용 |

UI는 `feat/core-siege`까지 OnGUI 디버그 패널로 대신한다 (결과 패널 제외).

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
- **UI는 최대한 뒤로:** `feat/core-siege`까지 OnGUI 디버그 패널로 수치를 보면 충분하다.
- **타워를 뒤로 미룬 이유:** 타워 종류와 건설 시 종류 선택 방법이 미정이다. 2단계는 이 결정 없이 진행할 수 있다.
- **병렬을 중단한 이유:** 레인을 동시에 열었더니 같은 파일과 씬 충돌이 잦아 정리 비용이 더 컸다.
- **PR 크기:** `feat/hero-combat`, `feat/core-siege`, `feat/tower`는 큰 편이라 리뷰가 길어지면 둘로 나눈다.

---

## 새 세션 시작 프롬프트 예시

```
To-steak/project_05 레포에서 작업한다.
CLAUDE.md, DESIGN.md, CONVENTIONS.md, ROADMAP.md를 먼저 읽어라.
ROADMAP.md에서 체크되지 않은 첫 항목을 main 최신에서 브랜치를 따서 구현하고 PR을 열어라.
```
