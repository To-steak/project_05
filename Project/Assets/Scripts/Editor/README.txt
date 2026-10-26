[Editor]
네임스페이스: Project05.Editor

에디터 전용 도구. 빌드에 들어가지 않는다 (어셈블리 includePlatforms = Editor). 게임 실행 코드(World, System, View)를 참조하지 않고, 씬에 타일과 Authoring 오브젝트를 만들거나 빌드 때 Definition 에셋을 손보는 것까지만 한다. 어셈블리 참조: Authoring.
네임스페이스가 Project05.Editor라 UnityEditor.Editor는 전체 이름으로 쓴다.

레벨 만드는 흐름: Level Tile Painter 창에서 타일 칠하기 → 스폰·코어·공터 찍기 → 경로 생성 → NavMesh 굽기.

들어갈 파일:
- LevelTileSet.cs (ScriptableObject) : 격자 설정(칸 크기, 원점, 타일 크기 배율, 길·풀 바닥 높이), 칠할 타일 프리팹, 길 타일 목록, 옵션(Batching Static, 클릭용 바닥 Collider).
- LevelGrid.cs (static class) : 칸 (x, z) ↔ 월드 위치 변환. 칸 (x, z) 중심 = origin + (x × cellSize, 0, z × cellSize). 상태 없음.
- LevelPaintMode.cs (enum) : 칠하기 창 모드 (Tile, Spawn, Core, TowerSlot).
- LevelTilePainterWindow.cs (EditorWindow) : Tools → Project05 → Level Tile Painter. 타일 모드: 썸네일을 골라 칠하기·교체·지우기(Shift), ] [ 회전, 썸네일 아래 "길" 체크. 스폰·공터 모드: 칸에 놓기·지우기(Shift). 코어 모드: 코어 옮기기. 경로 생성 버튼. 타일은 "Level Tiles (Painted)", 스폰·공터는 "Level Markers" 아래에 둔다. Scene 뷰 입력은 SceneView.duringSceneGui로 받는다 (에디터 전용이라 이벤트 금지 규칙 밖).
- LevelPathBuilder.cs (static class) : 길 타일이 칠해진 칸만 따라 스폰마다 코어 칸까지 최단 경로(너비 우선 탐색)를 찾아, 방향이 바뀌는 칸과 코어 앞 칸에 웨이포인트를 두고 SpawnPointAuthoring에 연결. 칠한 타일 범위를 덮는 클릭용 바닥 BoxCollider. "Level (Generated)" 아래를 다시 만들고, 밖의 WaypointPathAuthoring은 확인 후 지운다. 전체가 Undo 한 번.
- HeroPrefabBuildProcessor.cs (IPreprocessBuildWithReport, IPostprocessBuildWithReport) : 빌드 직전에 플랫폼에 맞지 않는 HeroDefinition 히어로 프리팹 칸(WebGL이면 desktopPrefab, 그 밖이면 webPrefab)을 비워 빌드에서 빼고, 빌드가 끝나면(실패해도 다음 에디터 프레임에) 되돌린다. 이번 플랫폼용 칸이 비어 있으면 비우지 않는다.

구현하면서 이 목록과 실제 파일을 맞춘다. 컨벤션은 저장소 루트의 CONVENTIONS.md를 따른다.
