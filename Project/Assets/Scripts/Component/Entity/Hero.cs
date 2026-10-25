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
        // 바라보는 방향 (XZ 단위 벡터). 이동한 틱에 이동 방향으로 바뀌고, 멈추면 유지한다. 근거리 부채꼴 판정과 View 회전에 쓴다
        public Vector3 facingDirection;
        // 이동 경로 코너 (NavMesh.CalculatePath 결과, 고정 용량 MAX_PATH_CORNER_COUNT)
        public Vector3[] pathCorners;
        // pathCorners에 들어 있는 코너 수
        public int pathCornerCount;
        // 다음에 향할 코너 인덱스
        public int nextPathCornerIndex;
        // 이동 속도 (유닛 / 틱)
        public float moveSpeed;
        // 마지막으로 경로를 계산한 이동 목표 (누르고 있는 동안 목표가 이만큼 바뀌어야 다시 계산)
        public Vector3 moveTargetPosition;
        // 누르고 이동할 때 경로를 다시 계산하는 목표 변화 거리
        public float moveRepathDistance;
        // 현재 체력
        public int currentHP;
        // 최대 체력
        public int maxHP;
        // 남은 목숨
        public int remainingLives;
        // 근거리 공격력
        public int meleeAttackDamage;
        // 근거리 공격 주기 (틱)
        public int meleeAttackIntervalTicks;
        // 다음 근거리 공격까지 남은 틱 (0이면 준비됨)
        public int meleeAttackCooldownTicks;
        // 근거리 사거리 x
        public float meleeRange;
        // 근거리 부채꼴 각도 (도, 기록·표시용)
        public float meleeAngleDegrees;
        // 근거리 부채꼴 판정용 cos(각도 / 2). 판 시작 시 각도에서 계산한다
        public float meleeHalfAngleCosine;
        // 원거리(구체) 공격력
        public int rangedAttackDamage;
        // 원거리(구체) 공격 주기 (틱)
        public int rangedAttackIntervalTicks;
        // 다음 원거리 공격까지 남은 틱 (0이면 준비됨)
        public int rangedAttackCooldownTicks;
        // 원거리 사거리 R (투사체 최대 비행 거리)
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
        // 지금 구체 수 (레벨로 매 틱 계산, DESIGN 3.1)
        public int orbCount;
        // 이전 틱의 첫 구체 각도 (도, View 보간용)
        public float previousOrbAngleDegrees;
        // 현재 틱의 첫 구체 각도 (도, 0 ~ 360). tickCount × orbDegreesPerTick으로 계산한다
        public float currentOrbAngleDegrees;
        // 원거리 투사체 이동 속도 (유닛 / 틱)
        public float projectileSpeed;
        // 원거리 투사체 명중 판정 반경
        public float projectileRadius;
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
