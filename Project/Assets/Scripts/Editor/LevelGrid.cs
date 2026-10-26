using UnityEngine;

namespace Project05.Editor
{
    /// <summary>
    /// 레벨 격자 칸과 월드 위치 변환. 칠하기 창과 경로 생성이 같은 계산을 쓰도록 모아 둔다. 상태 없음.
    /// 칸 (x, z)는 origin에서 +X로 x칸, +Z로 z칸 떨어진 칸이다.
    /// </summary>
    public static class LevelGrid
    {
        /// <summary>
        /// 칸 중심의 월드 위치 (높이는 origin.y).
        /// </summary>
        /// <param name="tileSet">격자 설정</param>
        /// <param name="cell">칸 (x, z)</param>
        /// <returns>칸 중심</returns>
        public static Vector3 GetCellCenter(LevelTileSet tileSet, Vector2Int cell)
        {
            return tileSet.origin + new Vector3(cell.x * tileSet.cellSize, 0f, cell.y * tileSet.cellSize);
        }

        /// <summary>
        /// 월드 위치가 속한 칸 (가장 가까운 칸 중심, 높이는 무시).
        /// </summary>
        /// <param name="tileSet">격자 설정</param>
        /// <param name="position">월드 위치</param>
        /// <returns>칸 (x, z)</returns>
        public static Vector2Int GetCell(LevelTileSet tileSet, Vector3 position)
        {
            return new Vector2Int(
                Mathf.RoundToInt((position.x - tileSet.origin.x) / tileSet.cellSize),
                Mathf.RoundToInt((position.z - tileSet.origin.z) / tileSet.cellSize));
        }
    }
}
