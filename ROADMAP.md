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

| 상태 | 브랜치 | 내용 | 확인 방법 |
|---|---|---|---|
| [x] | `refactor/foundation` | App과 기반 기능(SettingsStore, RecordStore, SceneLoader, AudioPlayer, ViewPool)을 Core에서 `Foundation/` 어셈블리로 분리. View·UI → Foundation 참조, System은 참조 금지 (CONVENTIONS 1.1). 폴더별 README.txt도 이동 | View·UI에서 `App.~` 사용 시 컴파일 성공, System에서는 실패 |
| [x] | `feat/hero-movement` | InputCollector, CommandSystem(이동), HeroSystem 이동, HeroViewSystem 보간, CameraViewSystem(Cinemachine 추적). 닫힌 PR #7 참고 | 클릭하면 히어로가 NavMesh 경로로 이동, 카메라가 따라옴 |
| [x] | `feat/enemy-wave` | WaveSystem(`x × 10`), SpawnSystem과 생성 요청 큐, EnemySystem 경로 이동, EnemyViewSystem 동기화와 풀 | 적이 웨이브마다 스폰돼 길을 따라 이동 |
| [x] | `feat/hero-combat` | TargetingSystem, 근거리 광역·원거리, 적 피해와 사망, 프레임 기록, 기본 연출 | 히어로가 적을 자동으로 처치 |
| [x] | `feat/core-siege` | 코어 포위와 공격, 접촉 피해, 무적, 목숨과 부활, GameFlow(준비·웨이브·보스·스테이지·패배), 결과 패널 | **한 판이 처음부터 패배까지 돌아감** |
| [x] | `feat/drop-progress` | 드롭, 골드·XP 획득, 레벨업 일시정지와 선택지 UI. 닫힌 PR #8 참고 | 레벨업 시 멈추고 선택 후 재개 |
| [x] | `feat/main-scene` | Main 메뉴, 설정(SettingsStore), 로딩 패널(SceneLoader), 최고 기록(RecordStore) | Main → 로딩 → Game 진입, 설정 저장 유지 |
| [x] | `feat/tower` | 공터 Space 건설, 자동 공격, 탑승과 클릭 사격, 타워 HP와 우선 공격, 파괴와 강제 하차 | 타워가 적을 막고 부서짐 |
| [x] | `feat/core-menu` | 코어 UI: 해금, 타워별 업그레이드, 수리, 일시정지 | 코어에서 Space로 메뉴 사용 |

---

## 4단계: 마무리

> 2단계와 같이 **한 번에 브랜치 하나만** 열고, 병합된 뒤 다음 항목을 시작한다.
> 연출(3~5)은 에셋이 정해진 뒤에 한다. 밸런싱은 타격감이 체감 난이도를 바꾸므로 연출 뒤에 한다.

이미 끝난 항목:
- [x] 화면 밖 적 화살표, Space 아이콘 (PR #23)
- [x] 카메라 수치와 원거리 사거리 제약 맞추기, Q/E 카메라 회전 (PR #24)

| 상태 | 순서 | 브랜치 | 내용 | 확인 방법 |
|---|---|---|---|---|
| [x] | 1 | (결정, 코드 없음) | 테마와 아트·사운드 에셋 출처 결정 (DESIGN 7장) | 결정됨 |
| [x] | 2 | `feat/hud` (PR #27) | OnGUI 디버그 패널을 대신하는 인게임 HUD: 히어로·코어 HP, 목숨, 골드, XP·레벨, 웨이브 `current/max`, 점수. View-Presenter 구조. 디버그 패널은 개발용으로 남긴다 | 디버그 패널을 접어도 한 판의 상태를 HUD만으로 읽을 수 있음 |
| [x] | 3 | `feat/audio` (PR #29) | `AudioPlayer`를 App에 추가하고 Bootstrap이 채움. BGM·SFX 재생, SettingsStore 볼륨 연결, 프레임 기록 → SFX. WebGL 첫 클릭 이후 재생 | 설정 볼륨이 즉시 반영되고, WebGL에서 첫 클릭 뒤 소리가 남 |
| [x] | 4 | `feat/vfx` (PR #30) | 프레임 기록 기반 VFX: 피격, 사망, 근거리 범위, 건설·파괴·수리·강화, 드롭 획득 "+10G" 표시, 코어 피격 화면 경고. 전부 `App.Pool`로 풀링 | 전투 중 VFX가 나오고, 반복 플레이에서 Instantiate가 늘지 않음 |
| [x] | 5 | `feat/animation` (PR #31) | Animator 연결. 이동은 Logic 속도 값, 공격·사망·부활은 프레임 기록으로 재생. 데미지 판정은 Logic 타이머 그대로 (DESIGN 5.12). 타워는 제외 | 히어로·적이 상태에 맞는 애니메이션을 재생 |
| [x] | 5.1 | `docs/hero-attack-split` (PR #32) | 히어로 공격 분리·구체·근거리 부채꼴·탑승 사격 광역 설계 반영 (DESIGN 3.1, 3.2, 3.6, 5.5, 5.7, 5.12, 7장) | 문서 병합 |
| [x] | 5.2 | `feat/hero-attack-split` (PR #33) | 근거리·원거리 타이머와 공격력 분리, 레벨업 선택지 분리, Logic 바라보는 방향(`facingDirection`), 근거리 전방 120° 부채꼴, 구체(개수·회전·발사 위치)와 구체 View, 부채꼴 범위 VFX, 탑승 중 히어로 공격 없음 | 이동하면 구체가 사방을 쏘고, 전방 적만 검에 맞음. 5레벨마다 구체가 늘어남 |
| [x] | 5.3 | `feat/boarded-splash` (PR #34) | 탑승 사격 공격력(타워 + (근거리 + 원거리) / 2), 명중·소진 지점 폭발 광역, 폭발 기록과 VFX | 탑승 사격이 맞은 지점 주변 적을 함께 맞힘 |
| [x] | 5.4 | `feat/level-layout-tool` (PR #35) | 에디터 전용 레벨 칠하기 창(Tools → Project05 → Level Tile Painter): 타일 칠하기, 스폰·코어·공터 배치, 칠한 길 타일 기반 경로·바닥 Collider 생성 | 칠하기 창으로 만든 레벨에서 적이 길을 따라 코어로 오고 히어로가 클릭 이동함 |
| [ ] | 6 | (사용자 작업) | 중간 WebGL 빌드 확인: 프레임, 압축 헤더, 탭 백그라운드 복귀 (DESIGN 6장) | itch.io에서 연출이 붙은 상태로 정상 동작 |
| [ ] | 7 | `balance/~` | 밸런싱. 아래 셋으로 나눠 PR을 연다 | 목표 플레이 시간 안에 패배에 도달 |
| [ ] | 8 | (사용자 작업) | 최종 WebGL 빌드와 itch.io 배포. DESIGN 6장 체크리스트 전부 체크 | 배포 페이지에서 처음부터 패배까지 플레이 가능 |

7번 밸런싱 PR 나누기:
1. `balance/base-stats` — 히어로·적 기본 수치, 타워 비용·업그레이드
   - **탑승 사격 가치:** 구체가 5개까지 늘면 원거리 화력이 5배가 되는데, 탑승하면 구체가 꺼지고 탑승 공격력은 `타워 + (근거리 + 원거리) / 2`라 레벨이 오를수록 탑승할 이유가 줄어든다. 탑승 공격력을 대폭 올린다. 후보: 구체 수에 비례(구체 화력이 타워로 옮겨 가는 의미), 또는 `boardedDamageMultiplier` 배율
   - **근거리 가치:** 적에게 붙는 위험(접촉 피해)이 있는데 한 방 피해가 원거리와 같다. 근거리 공격력을 원거리보다 크게(2~3배 후보) 둔다. 처치 보상·체력 흡수·넉백 같은 부가 효과는 새 규칙이라 설계 결정 후
2. `balance/difficulty-curve` — 증가 수식 계수(`hpGrowth` 등), 웨이브당 적 수(`x × 10` 임시 규칙 확정)
3. `balance/score-and-limits` — 점수 규칙 확정, 동시 적 수 상한

---

## 순서를 이렇게 잡은 이유

- **데이터 계약을 먼저 고정하는 이유:** 모든 브랜치가 `World.cs`와 Runner의 틱 연결을 고치면 병렬 작업 시 매번 충돌한다. Component와 빈 System을 먼저 다 만들어 두면, 이후 브랜치는 각자 맡은 System 파일의 내용만 채운다.
- **UI는 최대한 뒤로:** 기능 단계에서는 OnGUI 디버그 패널로 수치를 봤고, 인게임 HUD는 4단계에서 만들었다.
- **병렬을 중단한 이유:** 레인을 동시에 열었더니 같은 파일과 씬 충돌이 잦아 정리 비용이 더 컸다.
- **PR 크기:** `feat/hero-combat`, `feat/core-siege`, `feat/tower`는 큰 편이라 리뷰가 길어지면 둘로 나눈다.

---

## 새 세션 시작 프롬프트 예시

```
To-steak/project_05 레포에서 작업한다.
CLAUDE.md, DESIGN.md, CONVENTIONS.md, ROADMAP.md를 먼저 읽어라.
ROADMAP.md에서 체크되지 않은 첫 항목을 main 최신에서 브랜치를 따서 구현하고 PR을 열어라.
```
