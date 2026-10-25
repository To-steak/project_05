using UnityEngine;

namespace Project05.Editor
{
    /// <summary>
    /// 레벨 격자 원본. 인스펙터의 "레벨 생성" 버튼으로 LevelLayoutBuilder가 씬에 타일과 Authoring을 만든다. 에디터 전용이며 게임 데이터가 아니다.
    /// 격자 기호: '.' 풀밭, '#' 길, 'S' 적 스폰(길), 'C' 코어(길), 'T' 타워 공터(풀밭), '-' 빈칸. 공백은 무시한다.
    /// 첫 줄이 북쪽(+Z), 줄의 왼쪽이 서쪽(-X)이다. 첫 칸(왼쪽 위) 중심이 origin이다.
    /// 타일 기준 방향(회전 0도일 때 열린 방향): 직선 북·남, 굽은 길 북·동, 갈림길 북·동·남, 막다른 길 북.
    /// 프리팹이 이와 다르게 생겼으면 종류별 Yaw Offset(도, 시계 방향)으로 맞춘다. 타일은 generateTiles를 켤 때만 만든다.
    /// </summary>
    [CreateAssetMenu(fileName = "LevelLayout", menuName = "Project05/Level Layout")]
    public class LevelLayout : ScriptableObject
    {
        // 격자 (한 줄이 한 행. 기호는 클래스 설명 참고)
        [TextArea(8, 40)] public string grid = DEFAULT_GRID;
        // 칸 한 변의 길이 (월드 유닛)
        [Min(0.01f)] public float cellSize = 1f;
        // 첫 칸(왼쪽 위) 중심 위치
        public Vector3 origin;
        // 타일 프리팹에 곱할 크기 (타일이 칸 크기와 다를 때)
        [Min(0.01f)] public float tileScale = 1f;
        // 길 바닥 높이 (origin.y 기준). 스폰 지점과 웨이포인트를 이 높이에 둔다
        public float pathSurfaceHeight;
        // 풀 바닥 높이 (origin.y 기준). 공터와 클릭용 바닥 Collider 윗면을 이 높이에 둔다
        public float grassSurfaceHeight;

        [Header("Tiles")]
        // 풀밭 타일
        public GameObject grassPrefab;
        // 풀밭 타일 방향 보정 (도)
        public float grassYawOffset;
        // 직선 길 타일
        public GameObject straightPrefab;
        // 직선 길 방향 보정 (도. 0도에서 북·남이 열려야 한다)
        public float straightYawOffset;
        // 굽은 길 타일
        public GameObject bendPrefab;
        // 굽은 길 방향 보정 (도. 0도에서 북·동이 열려야 한다)
        public float bendYawOffset;
        // 갈림길 타일
        public GameObject splitPrefab;
        // 갈림길 방향 보정 (도. 0도에서 북·동·남이 열려야 한다)
        public float splitYawOffset;
        // 교차로 타일
        public GameObject crossPrefab;
        // 교차로 방향 보정 (도)
        public float crossYawOffset;
        // 막다른 길 타일
        public GameObject endPrefab;
        // 막다른 길 방향 보정 (도. 0도에서 북이 열려야 한다)
        public float endYawOffset;

        [Header("Options")]
        // 레벨 생성 때 타일도 만들지. 타일을 Level Tile Painter로 직접 칠한다면 끄고, 스폰·경로·공터·코어·바닥 Collider만 만든다
        public bool generateTiles;
        // 타일을 Batching Static으로 표시할지 (WebGL 그리기 호출 줄이기)
        public bool markTilesStatic = true;
        // 클릭 이동용 바닥 BoxCollider를 격자 전체에 하나 만들지 (InputCollector가 Physics.Raycast로 바닥을 찾는다)
        public bool createGroundCollider = true;

        private const string DEFAULT_GRID =
            ". . . . S . . . .\n" +
            ". . . . # . . . .\n" +
            ". . . T # T . . .\n" +
            "S # # # C . . . .\n" +
            ". . . T # T . . .\n" +
            ". . . . # . . . .\n" +
            ". . . . # # # . .\n" +
            ". . . . . . # . .\n" +
            ". . . . . . S . .";
    }
}
