using UnityEngine;

namespace Project05.Editor
{
    /// <summary>
    /// 타일 칠하기 창(LevelTilePainterWindow)에서 고를 타일 프리팹 목록. 에디터 전용이며 게임 데이터가 아니다.
    /// Project 창에서 여러 FBX·프리팹을 한꺼번에 끌어 목록에 놓을 수 있다.
    /// </summary>
    [CreateAssetMenu(fileName = "LevelTileSet", menuName = "Project05/Level Tile Set")]
    public class LevelTileSet : ScriptableObject
    {
        // 칠할 타일 프리팹 (FBX 모델도 된다)
        public GameObject[] prefabs = new GameObject[0];
    }
}
