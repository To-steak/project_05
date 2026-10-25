namespace Project05.Component.Data
{
    /// <summary>
    /// 히어로 수치 테이블. HeroDefinition에서 베이킹되며, 판 시작 시 히어로에 복사한다.
    /// </summary>
    public struct HeroStats
    {
        // 최대 체력
        public int maxHP;
        // 시작 목숨 수
        public int lifeCount;
        // 이동 속도 (유닛 / 틱)
        public float moveSpeed;
        // 누르고 이동할 때 경로를 다시 계산하는 목표 변화 거리
        public float moveRepathDistance;
        // 근거리 공격력
        public int meleeAttackDamage;
        // 근거리 공격 주기 (틱)
        public int meleeAttackIntervalTicks;
        // 근거리 사거리 x
        public float meleeRange;
        // 근거리 부채꼴 각도 (도)
        public float meleeAngleDegrees;
        // 원거리(구체) 공격력
        public int rangedAttackDamage;
        // 원거리(구체) 공격 주기 (틱)
        public int rangedAttackIntervalTicks;
        // 원거리 사거리 R (투사체 최대 비행 거리이기도 하다)
        public float rangedRange;
        // 시작 구체 수
        public int orbBaseCount;
        // 구체가 하나 늘어나는 레벨 간격
        public int orbLevelStep;
        // 최대 구체 수
        public int orbMaxCount;
        // 구체 회전 반지름
        public float orbRadius;
        // 구체 회전 속도 (도 / 틱)
        public float orbDegreesPerTick;
        // 원거리 투사체 이동 속도 (유닛 / 틱)
        public float projectileSpeed;
        // 원거리 투사체 명중 판정 반경 (적 반경과 더해 판정)
        public float projectileRadius;
        // 접촉 판정 반경 (적 반경과 더해 판정)
        public float contactRadius;
        // 피격 후 무적 시간 (틱)
        public int invincibilityTicks;
        // 사망 후 부활까지 걸리는 시간 (틱)
        public int respawnTicks;
        // 드롭 획득 반경
        public float pickupRadius;
        // Space 상호작용 반경 (공터, 타워 탑승, 코어)
        public float interactionRadius;
    }
}
