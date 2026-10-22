[Component/Event]
네임스페이스: Project05.Component.Event

Logic → View 이벤트. struct이며 발생 시점의 값을 복사해 담는다. 객체 참조 금지.

들어갈 파일:
- EventQueue.cs (class) : 이벤트 저장소. (이벤트 종류별 큐 / 단일 union struct 중 방식은 구현 전 결정)
- EnemySpawnedEvent.cs (struct) : EntityID, 적 종류, 위치.
- EnemyDamagedEvent.cs (struct) : EntityID, 위치, 피해량.
- EnemyDiedEvent.cs (struct) : EntityID, 적 종류, 위치.
- EnemyReachedCoreEvent.cs (struct) : EntityID, 위치.
- HeroAttackedEvent.cs (struct) : 공격 종류, 위치, 대상 위치.
- HeroDiedEvent.cs (struct) : 위치.
- HeroRespawnedEvent.cs (struct) : 위치.
- HeroBoardedEvent.cs (struct) : 타워 EntityID.
- HeroUnboardedEvent.cs (struct) : 타워 EntityID.
- TowerBuiltEvent.cs (struct) : 타워 EntityID, 종류, 슬롯 인덱스, 위치.
- TowerUpgradedEvent.cs (struct) : 타워 EntityID, 단계.
- TowerFiredEvent.cs (struct) : 타워 EntityID, 위치, 대상 위치, 탑승 사격 여부.
- DropSpawnedEvent.cs (struct) : EntityID, 종류, 위치.
- DropCollectedEvent.cs (struct) : EntityID, 종류, 양.
- LevelUpEvent.cs (struct) : 새 레벨, 선택지.
- CoreDamagedEvent.cs (struct) : 피해량, 남은 HP.
- WaveStartedEvent.cs (struct) : 웨이브 번호.
- GameEndedEvent.cs (struct) : 승리 여부.

구현하면서 이 목록과 실제 파일을 맞춘다. 컨벤션은 저장소 루트의 CONVENTIONS.md를 따른다.
