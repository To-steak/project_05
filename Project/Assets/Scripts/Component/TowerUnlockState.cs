namespace Project05.Component
{
    /// <summary>
    /// 타워 종류별 해금 상태. 타워 종류 목록이 미정이라 종류 인덱스 배열로 둔다.
    /// </summary>
    public struct TowerUnlockState
    {
        // 타워 종류별 해금 여부 (인덱스 = TowerStats 테이블 인덱스)
        public bool[] isUnlockedByTowerType;
    }
}
