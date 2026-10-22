[Component]
네임스페이스: Project05.Component

순수 데이터. 로직(함수)을 두지 않는다. 어셈블리 참조: 없음.

들어갈 파일:
- World.cs (class) : 한 판의 모든 상태. 엔티티 저장소, 자원, 웨이브 진행, 입력 버퍼, 이벤트 큐, 시드 고정 랜덤, 베이킹 테이블 참조를 가진다.
- EntityID.cs (struct) : 엔티티 식별자. index와 generation.
- EntityStore.cs (class) : 엔티티 배열 + 빈 슬롯 목록 + generation 관리. (제네릭 여부는 구현 시 결정)
- GamePhase.cs (enum) : Preparing, WaveInProgress, Victory, Defeat
- GameFlowState.cs (struct) : 현재 GamePhase, 단계 전환까지 남은 틱.
- CoreState.cs (struct) : 코어(기지) 위치, 최대 HP, 현재 HP. (`Core`는 Project05.Core 네임스페이스와 충돌하므로 사용하지 않음)
- ProgressState.cs (struct) : 골드, 현재 XP, 레벨, 대기 중인 레벨업 선택지.
- RandomState.cs (class) : 시드 고정 global::System.Random 래퍼. (World 필드로 둘 경우 생략 가능)

구현하면서 이 목록과 실제 파일을 맞춘다. 컨벤션은 저장소 루트의 CONVENTIONS.md를 따른다.
