namespace Project05.Component.Data
{
    /// <summary>
    /// 타워 종류별 수치 테이블. TowerDefinition에서 베이킹되며, 건설·업그레이드 시 타워에 복사한다.
    /// </summary>
    public struct TowerStats
    {
        // 종류 해금 비용 (골드)
        public int unlockCost;
        // 건설 비용 (골드)
        public int buildCost;
        // 수리 비용 (골드)
        public int repairCost;
        // 적이 경로를 멈추고 이 타워를 공격하기 시작하는 거리
        public float aggroRange;
        // 타워 포위: 가장 안쪽 고리 반지름 (고리·자리 간격은 코어와 같다)
        public float siegeFirstRingRadius;
        // 투사체 이동 속도 (유닛 / 틱)
        public float projectileSpeed;
        // 투사체 명중 판정 반경
        public float projectileRadius;
        // 탑승 사격 폭발 반경
        public float boardedSplashRadius;
        // 단계별 수치 (0번이 건설 직후 단계)
        public TowerLevelStats[] levels;
    }
}
