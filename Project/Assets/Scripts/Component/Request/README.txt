[Component/Request]
네임스페이스: Project05.Component.Request

생성 요청. 다른 System이 World의 종류별 Queue에 넣고, SpawnSystem(틱 마지막)이 처리한다. 이번 틱 요청은 다음 틱부터 동작.

들어갈 파일:
- EnemySpawnRequest.cs (struct) : 적 종류, 스폰 지점 인덱스, 누적 웨이브 번호, 보스 여부.
- DropSpawnRequest.cs (struct) : 종류(골드/XP), 양, 위치.
- TowerBuildRequest.cs (struct) : 공터 인덱스, 타워 종류.
- ProjectileSpawnRequest.cs (struct) : 발사 위치, 방향, 속도, 명중 반경, 최대 비행 거리, 피해량, 폭발 반경(0이면 단일 명중).

구현하면서 이 목록과 실제 파일을 맞춘다. 컨벤션은 저장소 루트의 CONVENTIONS.md를 따른다.
