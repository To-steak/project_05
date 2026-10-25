[Editor]
네임스페이스: Project05.Editor

에디터 전용 도구. 빌드에 들어가지 않는다 (어셈블리 includePlatforms = Editor). 게임 실행 코드(World, System, View)를 참조하지 않고, 씬에 Authoring 오브젝트를 만들어 주는 것까지만 한다. 어셈블리 참조: Authoring.
네임스페이스가 Project05.Editor라 UnityEditor.Editor는 전체 이름으로 쓴다.

들어갈 파일:
- LevelLayout.cs (ScriptableObject) : 레벨 격자 텍스트, 칸 크기, 원점, 길·풀 바닥 높이, 타일 종류별 프리팹과 기준 방향 보정값, 생성 옵션.
- LevelTileKind.cs (enum) : 격자 칸에 놓을 타일 종류 (Grass, Straight, Bend, Split, Cross, End).
- LevelLayoutBuilder.cs (static class) : 격자 해석·검사, 이웃 칸으로 타일 종류·방향 결정, 타일 배치, 스폰·경로·웨이포인트·공터 Authoring 생성, 코어 이동, 클릭용 바닥 Collider. 전부 Undo 한 번으로 되돌린다.
- LevelLayoutEditor.cs (CustomEditor) : LevelLayout 인스펙터에 기호 설명, 검사 결과, 레벨 생성 버튼.

구현하면서 이 목록과 실제 파일을 맞춘다. 컨벤션은 저장소 루트의 CONVENTIONS.md를 따른다.
