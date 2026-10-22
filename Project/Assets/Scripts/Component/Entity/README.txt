[Component/Entity]
네임스페이스: Project05.Component.Entity

엔티티 struct. 생성 시 테이블 값을 복사해 자기 데이터로 가진다.

들어갈 파일:
- Hero.cs (struct) : 위치(previousPosition, currentPosition), 이동 경로 코너, HP, 공격 타이머, 사거리, 탑승 중인 타워 EntityID, 부활 대기 틱.
- Enemy.cs (struct) : 위치, 경로 인덱스, 다음 웨이포인트 인덱스, HP, 이동 속도, 코어 피해량, 드롭 골드·XP.
- Tower.cs (struct) : 위치, 슬롯 인덱스, 타워 종류, 강화 단계, 공격력, 사거리, 공격 타이머, 탑승 여부.
- TowerSlot.cs (struct) : 위치, 건설된 타워 EntityID.
- Drop.cs (struct) : 위치, 종류(골드/XP), 양.
- DropKind.cs (enum) : Gold, XP
- HeroAttackKind.cs (enum) : Melee, Ranged

구현하면서 이 목록과 실제 파일을 맞춘다. 컨벤션은 저장소 루트의 CONVENTIONS.md를 따른다.
