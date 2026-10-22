namespace Project05.Component.Request
{
    /// <summary>
    /// 타워 건설 요청. 비용은 CommandSystem이 요청 전에 지불한다.
    /// </summary>
    public struct TowerBuildRequest
    {
        // 공터 인덱스 (World.towerSlots)
        public int slotIndex;
        // 타워 종류 (TowerStats 테이블 인덱스)
        public int towerTypeIndex;
    }
}
