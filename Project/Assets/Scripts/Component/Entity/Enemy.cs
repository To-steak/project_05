using UnityEngine;

namespace Project05.Component.Entity
{
    /// <summary>
    /// 적. 스폰 시 EnemyStats에 난이도 수식을 적용한 값을 복사한다. 보스도 같은 struct를 쓴다.
    /// </summary>
    public struct Enemy
    {
        // 적 종류 (EnemyStats 테이블 인덱스)
        public int enemyTypeIndex;
        // 현재 상태
        public EnemyState state;
        // 이전 틱의 위치 (View 보간용)
        public Vector3 previousPosition;
        // 현재 틱의 위치
        public Vector3 currentPosition;
        // 따라가는 경로 인덱스 (World.waypointPaths)
        public int pathIndex;
        // 다음에 향할 웨이포인트 인덱스
        public int nextWaypointIndex;
        // 현재 체력
        public int currentHP;
        // 최대 체력
        public int maxHP;
        // 이동 속도 (유닛 / 틱)
        public float moveSpeed;
        // 접촉 판정 반경
        public float radius;
        // 히어로 접촉 피해
        public int contactDamage;
        // 코어 공격력
        public int coreDamage;
        // 타워 공격력
        public int towerDamage;
        // 코어·타워 공격 주기 (틱)
        public int attackIntervalTicks;
        // 다음 공격까지 남은 틱
        public int attackCooldownTicks;
        // 공격 중인 타워 ID (없으면 무효 ID)
        public EntityID targetTowerID;
        // 배정받은 포위 자리 번호 (코어 또는 대상 타워 주변, 없으면 -1)
        public int siegeSlotIndex;
        // 사망 시 드롭 골드 (난이도 적용 후)
        public int dropGold;
        // 사망 시 드롭 XP (난이도 적용 후)
        public int dropXP;
        // 보스 여부
        public bool isBoss;
    }
}
