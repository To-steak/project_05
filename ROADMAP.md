## 0단계: 코드 전 준비 (main)

- 남은 문서 정리, 그리고 말씀하신 README.txt 일괄 갱신 (Component/Event → Record 등)
- Project Settings: Fixed Timestep 0.02, Maximum Allowed Timestep 0.1
- **빈 씬으로 WebGL 빌드 한 번:** 배포 경로와 압축 설정 문제를 지금 잡아두면 막판 사고를 막아요.

## 1단계: 뼈대 (움직이는 걸 볼 때까지)

| 순서 | 브랜치 | 내용 | 확인 방법 |
| --- | --- | --- | --- |
| 1 | `chore/assembly-setup` | 폴더별 Assembly Definition, 참조 방향 | System에서 View를 참조하면 컴파일 에러 |
| 2 | `feat/core-loop` | Bootstrap, Runner, App, World, EntityID, EntityStore, SessionSystem | Game 씬 Play 시 Runner 생성, 틱 카운트 증가 |
| 3 | `feat/hero-movement` | 입력 수집, CommandSystem(이동), HeroSystem 이동, HeroViewSystem 보간, Cinemachine 추적 | 클릭하면 캡슐이 NavMesh 경로로 이동, 카메라가 따라옴 |

3번이 끝나면 **WebGL 빌드를 한 번 더** 올려서 구조 전체가 브라우저에서 도는지 확인하는 걸 추천해요.

## 2단계: 게임이 되는 최소 루프

| 순서 | 브랜치 | 내용 | 확인 방법 |
| --- | --- | --- | --- |
| 4 | `feat/baking` | Hero·Enemy·Progression Definition, DefinitionBaker, SceneBaker(웨이포인트, 스폰, 코어, 공터) | Play 시 World에 베이킹된 값이 들어감 (디버그 표시) |
| 5 | `feat/enemy-wave` | WaveSystem(`x × 10`), SpawnSystem, EnemySystem 경로 이동, EnemyViewSystem 동기화와 풀 | 적이 웨이브마다 스폰돼 길을 따라 이동 |
| 6 | `feat/hero-combat` | TargetingSystem, 근거리 광역·원거리, 적 피해와 사망, 프레임 기록, 기본 연출 | 히어로가 적을 자동으로 처치 |
| 7 | `feat/core-siege` | 코어 포위와 공격, 접촉 피해, 무적, 목숨과 부활, GameFlow(준비, 웨이브, 보스, 스테이지, 패배), 결과 패널 | **처음부터 끝까지 한 판이 돌아감** |

7번이 끝나면 타워 없이도 "싸우다 지는" 한 판이 완성돼요. 여기가 가장 중요한 중간 지점이에요.

## 3단계: 성장과 타워

| 순서 | 브랜치 | 내용 | 확인 방법 |
| --- | --- | --- | --- |
| 8 | `feat/drop-progress` | 드롭, 골드·XP 획득, 레벨업 일시정지, 선택지 UI | 레벨업 시 멈추고 선택 후 재개 |
| 9 | `feat/tower` | 공터 Space 건설, 자동 공격, 탑승과 클릭 사격, 타워 HP와 우선 공격, 파괴와 강제 하차 | 타워가 적을 막고 부서짐 |
| 10 | `feat/core-menu` | 코어 UI: 해금, 업그레이드, 수리, 일시정지 | 코어에서 Space로 메뉴 사용 |
| 11 | `feat/main-scene` | Main 메뉴, 설정, 로딩 패널, 최고 기록 | Main → Game → 결과 → Main 전체 흐름 |

## 4단계: 마무리

- 화면 밖 적 화살표, Space 아이콘, 연출(VFX, SFX, 애니메이션)
- 카메라 수치와 원거리 사거리 제약 맞추기
- 밸런싱 (증가 계수, 비용, 점수 규칙)
- 최종 WebGL 빌드와 배포

## 순서를 이렇게 잡은 이유

- **UI는 최대한 뒤로:** 5~7단계는 OnGUI 디버그 패널로 수치를 보면 충분해요. UI를 먼저 만들면 구조가 바뀔 때 같이 고쳐야 해요.
- **타워를 뒤로 미룬 이유:** 아직 타워 종류와 건설 시 종류 선택 방법이 미정이에요. 1~8단계는 이 결정 없이 진행할 수 있어서, 작업하는 동안 천천히 정하시면 돼요.
- **PR 크기:** 6, 7, 9번은 큰 편이라 리뷰하시다가 길다 싶으면 둘로 나눌게요.