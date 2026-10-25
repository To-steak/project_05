[Component/Record]
네임스페이스: Project05.Component.Record

프레임 기록. 그 순간의 일을 Logic이 World의 종류별 List에 추가하고, ViewSystem이 LateUpdate에서 읽어 연출하며, Runner가 마지막에 비운다. struct이며 값만 복사해 담는다 (객체 참조 금지). 이벤트가 아니다 (구독 없음).

들어갈 파일:
- WaveStartedRecord.cs (struct) : 스테이지, 웨이브.
- WaveClearedRecord.cs (struct) : 스테이지, 웨이브.
- BossSpawnedRecord.cs (struct) : 보스 EntityID, 위치.
- StageClearedRecord.cs (struct) : 스테이지.
- HeroMeleeAttackRecord.cs (struct) : 위치, 방향, 반경, 부채꼴 각도.
- HeroRangedShotRecord.cs (struct) : 시작 위치(구체 위치), 대상 EntityID, 대상 위치. 구체마다 하나.
- HeroDamagedRecord.cs (struct) : 위치, 피해량.
- HeroDiedRecord.cs (struct) : 위치, 남은 목숨.
- HeroRespawnedRecord.cs (struct) : 위치.
- TowerBuiltRecord.cs (struct) : 타워 EntityID, 위치.
- TowerUpgradedRecord.cs (struct) : 타워 EntityID, 단계.
- TowerShotRecord.cs (struct) : 타워 EntityID, 대상 위치, 탑승 사격 여부.
- ProjectileExplodedRecord.cs (struct) : 폭발 중심, 폭발 반경 (탑승 사격 폭발).
- TowerDamagedRecord.cs (struct) : 타워 EntityID, 위치, 피해량.
- TowerDestroyedRecord.cs (struct) : 타워 EntityID, 위치.
- TowerRepairedRecord.cs (struct) : 타워 EntityID.
- EnemyDamagedRecord.cs (struct) : 적 EntityID, 위치, 피해량.
- EnemyDiedRecord.cs (struct) : 적 EntityID, 종류, 위치.
- EnemyCoreAttackRecord.cs (struct) : 적 EntityID, 위치.
- EnemyTowerAttackRecord.cs (struct) : 적 EntityID, 타워 EntityID, 위치.
- DropCollectedRecord.cs (struct) : 종류, 양, 위치.
- CoreDamagedRecord.cs (struct) : 피해량.
- CommandRejectedRecord.cs (struct) : 명령 종류, 사유 (골드 부족, 범위 밖 등).
- CommandRejectReason.cs (enum) : NotEnoughGold, OutOfRange, NotUnlocked, SlotOccupied, AlreadyUnlocked, MaxLevel, AlreadyFullHP, NoTower.

구현하면서 이 목록과 실제 파일을 맞춘다. 컨벤션은 저장소 루트의 CONVENTIONS.md를 따른다.
