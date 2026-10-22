[Component]
네임스페이스: Project05.Component

순수 데이터. 로직(함수)을 두지 않는다. 어셈블리 참조: 없음.

들어갈 파일:
- World.cs (class) : 한 판의 모든 상태. 엔티티 저장소, 히어로, 코어, 게임 흐름, 진행 상태, 입력 버퍼, 생성 요청 큐, 프레임 기록 목록, 시드 고정 랜덤, 베이킹 테이블 참조, 일시정지 상태를 가진다.
- EntityID.cs (struct) : 엔티티 식별자. index와 generation.
- EntityStore.cs (class) : 제네릭 EntityStore<T> where T : struct. 고정 용량 배열 + 빈 자리 스택 + generation. TryCreate, ref Get, IsAlive, Destroy, 순회. 용량: 적 256, 드롭 256, 타워 = 공터 수.
- GamePhase.cs (enum) : Preparing, WaveInProgress, BossInProgress, Defeat. (무한 스테이지이므로 승리 없음)
- PauseReason.cs (enum) : None, LevelUpChoice, CoreMenu. None이 아니면 CommandSystem만 실행.
- GameFlowState.cs (struct) : GamePhase, 스테이지 n, 웨이브 x, 처치 수 current, 웨이브 적 수 max (= x × 10), 스폰한 수, 단계 전환까지 남은 틱, 다음 스폰까지 남은 틱, 보스 EntityID.
- CoreState.cs (struct) : 코어 위치, 최대 HP, 현재 HP, 포위 자리 사용 현황. (`Core`는 Project05.Core 네임스페이스와 충돌하므로 사용하지 않음)
- ProgressState.cs (struct) : 골드, 현재 XP, 레벨, 대기 중인 레벨업 수, 현재 제시된 선택지 3개, 골드 획득 퍼센트, XP 획득 퍼센트, 점수.
- TowerUnlockState.cs (struct) : 타워 종류별 해금 여부. 종류 목록이 미정이라 타워 종류 인덱스 bool 배열로 둔다.
- (RandomState는 만들지 않는다. 시드 고정 랜덤은 World.random 필드로 둔다.)

구현하면서 이 목록과 실제 파일을 맞춘다. 컨벤션은 저장소 루트의 CONVENTIONS.md를 따른다.
