namespace Project05.Component.Data
{
    /// <summary>
    /// 적 종류별 수치 테이블. EnemyDefinition에서 베이킹되며, 스폰 시 난이도 수식을 적용해 적에 복사한다.
    /// </summary>
    public struct EnemyStats
    {
        // 기본 체력 (난이도 수식 적용 전)
        public int baseHP;
        // 이동 속도 (유닛 / 틱)
        public float moveSpeed;
        // 접촉 판정 반경 (히어로 반경과 더해 판정)
        public float radius;
        // 히어로 접촉 피해
        public int contactDamage;
        // 코어 공격력
        public int coreDamage;
        // 타워 공격력
        public int towerDamage;
        // 코어·타워 공격 주기 (틱)
        public int attackIntervalTicks;
        // 기본 드롭 골드 (난이도 수식 적용 전)
        public int baseDropGold;
        // 기본 드롭 XP (난이도 수식 적용 전)
        public int baseDropXP;
        // 보스 여부
        public bool isBoss;
        // 처치 점수
        public int killScore;
    }
}
