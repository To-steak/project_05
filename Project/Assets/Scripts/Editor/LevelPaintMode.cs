namespace Project05.Editor
{
    /// <summary>
    /// 타일 칠하기 창에서 Scene 뷰 클릭이 하는 일.
    /// </summary>
    public enum LevelPaintMode
    {
        // 타일 칠하기·지우기
        Tile,
        // 적 스폰 지점 놓기·지우기
        Spawn,
        // 코어 옮기기
        Core,
        // 타워 공터 놓기·지우기
        TowerSlot,
    }
}
