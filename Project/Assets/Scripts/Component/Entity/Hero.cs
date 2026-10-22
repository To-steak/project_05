using UnityEngine;

namespace Project05.Component.Entity
{
    /// <summary>
    /// 히어로. 하나뿐이므로 저장소 없이 World 필드로 둔다. 판 시작 시 HeroStats 값을 복사한다.
    /// </summary>
    public struct Hero
    {
        // 현재 상태
        public HeroState state;
        // 이전 틱의 위치 (View 보간용)
        public Vector3 previousPosition;
        // 현재 틱의 위치
        public Vector3 currentPosition;
        // 이동 경로 코너 (NavMesh.CalculatePath 결과, 고정 용량 MAX_PATH_CORNER_COUNT)
        public Vector3[] pathCorners;
        // pathCorners에 들어 있는 코너 수
        public int pathCornerCount;
        // 다음에 향할 코너 인덱스
        public int nextPathCornerIndex;
        // 이동 속도 (유닛 / 틱)
        public float moveSpeed;
        // 현재 체력
        public int currentHP;
        // 최대 체력
        public int maxHP;
        // 남은 목숨
        public int remainingLives;
        // 공격력 (탑승 시 타워 공격력)
        public int attackDamage;
        // 공격 주기 (틱)
        public int attackIntervalTicks;
        // 다음 공격까지 남은 틱 (0이면 준비됨)
        public int attackCooldownTicks;
        // 근거리 광역 사거리 x
        public float meleeRange;
        // 원거리 사거리 R
        public float rangedRange;
        // 접촉 판정 반경
        public float contactRadius;
        // 피격 후 무적 시간 (틱)
        public int invincibilityTicks;
        // 남은 무적 시간 (틱)
        public int invincibilityRemainingTicks;
        // 부활까지 걸리는 시간 (틱)
        public int respawnTicks;
        // 부활까지 남은 시간 (틱)
        public int respawnRemainingTicks;
        // 드롭 획득 반경
        public float pickupRadius;
        // Space 상호작용 반경
        public float interactionRadius;
        // 탑승 중인 타워 ID (탑승 중이 아니면 무효 ID)
        public EntityID boardedTowerID;

        // 경로 코너 배열 용량
        public const int MAX_PATH_CORNER_COUNT = 64;
    }
}
