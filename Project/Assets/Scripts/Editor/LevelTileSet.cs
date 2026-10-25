using UnityEngine;

namespace Project05.Editor
{
    /// <summary>
    /// 레벨 칠하기 설정. 격자(칸 크기·원점·바닥 높이), 칠할 타일 프리팹, 적이 다니는 길 타일 목록을 담는다. 에디터 전용이며 게임 데이터가 아니다.
    /// 타일 칠하기 창(LevelTilePainterWindow)과 경로 생성(LevelPathBuilder)이 같은 격자를 쓴다.
    /// 칸 (x, z)의 중심 = origin + (x × cellSize, 0, z × cellSize).
    /// </summary>
    [CreateAssetMenu(fileName = "LevelTileSet", menuName = "Project05/Level Tile Set")]
    public class LevelTileSet : ScriptableObject
    {
        [Header("Grid")]
        // 칸 한 변의 길이 (월드 유닛, 타일 한 변과 같게)
        [Min(0.01f)] public float cellSize = 1f;
        // 격자 기준점. 이 위치가 칸 (0, 0)의 중심이고, 높이는 칠하는 바닥면이다
        public Vector3 origin;
        // 타일 프리팹에 곱할 크기 (타일이 칸 크기와 다를 때)
        [Min(0.01f)] public float tileScale = 1f;
        // 길 바닥 높이 (origin.y 기준). 스폰 지점과 웨이포인트를 이 높이에 둔다
        public float pathSurfaceHeight;
        // 풀 바닥 높이 (origin.y 기준). 공터와 클릭용 바닥 Collider 윗면을 이 높이에 둔다
        public float grassSurfaceHeight;

        [Header("Tiles")]
        // 칠할 타일 프리팹 (FBX 모델도 된다). Project 창에서 여러 개를 한꺼번에 끌어 넣을 수 있다
        public GameObject[] prefabs = new GameObject[0];
        // 적이 다니는 길 타일. 경로 생성은 이 타일이 칠해진 칸만 따라간다 (칠하기 창의 "길" 체크로 바꾼다)
        public GameObject[] pathPrefabs = new GameObject[0];

        [Header("Options")]
        // 칠한 타일을 Batching Static으로 표시할지 (WebGL 그리기 호출 줄이기)
        public bool markTilesStatic = true;
        // 경로 생성 때 칠한 타일 범위를 덮는 클릭용 바닥 BoxCollider를 만들지 (InputCollector가 Physics.Raycast로 바닥을 찾는다)
        public bool createGroundCollider = true;
    }
}
