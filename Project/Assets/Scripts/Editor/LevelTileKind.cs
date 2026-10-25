namespace Project05.Editor
{
    /// <summary>
    /// 레벨 격자 칸에 놓을 타일 종류. 길 칸은 이웃한 길 칸(상하좌우)으로 종류와 방향이 정해진다.
    /// </summary>
    public enum LevelTileKind
    {
        // 풀밭 (길이 아닌 칸, 공터 칸 포함)
        Grass,
        // 직선 길 (마주 보는 두 방향이 열림)
        Straight,
        // 굽은 길 (이웃한 두 방향이 열림)
        Bend,
        // T자 갈림길 (세 방향이 열림)
        Split,
        // 교차로 (네 방향이 열림)
        Cross,
        // 막다른 길 (한 방향만 열림)
        End,
    }
}
