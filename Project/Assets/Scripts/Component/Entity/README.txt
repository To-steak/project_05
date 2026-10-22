[Component/Entity]
네임스페이스: Project05.Component.Entity

엔티티 struct. 생성(SpawnSystem) 시 테이블 값을 복사해 자기 데이터로 가진다. 매 틱 로직은 자기 값만 쓴다.

들어갈 파일:
- Hero.cs (struct) : previousPosition, currentPosition, 이동 경로 코너, 현재·최대 HP, 목숨, 공격력, 공격 타이머, 근거리·원거리 사거리, 투사체 속도·반경, 무적 남은 틱, 부활 남은 틱, 탑승 중인 타워 EntityID.
- HeroState.cs (enum) : Moving 또는 Idle, Boarded, Dead (부활 대기).
- Enemy.cs (struct) : previousPosition, currentPosition, 경로 인덱스, 다음 웨이포인트 인덱스, 현재·최대 HP, 이동 속도, 접촉 반경, 접촉 피해, 코어·타워 공격력과 공격 주기 틱, 공격 타이머, 상태, 공격 대상 타워 EntityID, 포위 자리, 드롭 골드·XP, 보스 여부.
- EnemyState.cs (enum) : FollowingPath, AttackingTower, MovingToSiegeSlot (경로 끝 → 포위 자리), AttackingCore.
- Tower.cs (struct) : 위치, 공터 인덱스, 타워 종류, 업그레이드 단계, 현재·최대 HP, 공격력, 사거리, 공격 주기 틱, 공격 타이머, aggroRange, 포위 자리 사용 현황, 탑승 여부.
- TowerSlot.cs (struct) : 공터 위치, 건설된 타워 EntityID (비어 있으면 무효 ID), 건설 요청 대기 여부.
- Drop.cs (struct) : 위치, 종류(골드/XP), 양.
- DropKind.cs (enum) : Gold, XP
- HeroAttackKind.cs (enum) : Melee, Ranged
- Projectile.cs (struct) : previousPosition, currentPosition, 방향, 속도, 명중 반경, 남은 비행 거리, 피해량. 직선 비행, 이동 구간의 첫 적 하나에 명중.

구현하면서 이 목록과 실제 파일을 맞춘다. 컨벤션은 저장소 루트의 CONVENTIONS.md를 따른다.
