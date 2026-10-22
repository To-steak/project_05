[Authoring/Scene]
네임스페이스: Project05.Authoring.Scene

Game 씬에 배치하는 MonoBehaviour. 판 시작 시 위치 등을 World로 베이킹한다. 실행 중 로직을 갖지 않는다.

들어갈 파일:
- TowerSlotAuthoring.cs (MonoBehaviour) : 타워 설치 슬롯 위치.
- WaypointPathAuthoring.cs (MonoBehaviour) : 자식 Transform 순서로 웨이포인트 경로 정의, Gizmo 표시.
- SpawnPointAuthoring.cs (MonoBehaviour) : 적 스폰 위치와 연결 경로.
- CoreAuthoring.cs (MonoBehaviour) : 코어 위치.
- HeroSpawnAuthoring.cs (MonoBehaviour) : 히어로 시작·부활 위치.

구현하면서 이 목록과 실제 파일을 맞춘다. 컨벤션은 저장소 루트의 CONVENTIONS.md를 따른다.
