using UnityEngine;
using UnityEngine.Serialization;

namespace Project05.Authoring.Definition
{
    /// <summary>
    /// 히어로 수치와 프리팹. 게임 시작 시 DefinitionBaker가 HeroStats로 베이킹한다.
    /// 시간은 틱(1틱 = 20ms), 거리는 월드 유닛, 속도는 유닛 / 틱이다.
    /// </summary>
    [CreateAssetMenu(fileName = "HeroDefinition", menuName = "Project05/Definition/Hero")]
    public class HeroDefinition : ScriptableObject
    {
        // 최대 체력
        [Min(1)] public int maxHP = 1;
        // 시작 목숨 수
        [Min(1)] public int lifeCount = 3;
        // 이동 속도 (유닛 / 틱)
        [Min(0f)] public float moveSpeed;
        // 마우스를 누르고 이동할 때, 커서 지점이 이 거리 이상 바뀌면 경로를 다시 계산한다
        [Min(0.05f)] public float moveRepathDistance = 0.5f;
        // 근거리 공격력
        [Min(0)] public int meleeAttackDamage;
        // 근거리 공격 주기 (틱)
        [Min(1)] public int meleeAttackIntervalTicks = 1;
        // 근거리 사거리 x
        [Min(0f)] public float meleeRange;
        // 근거리 부채꼴 각도 (도, 바라보는 방향 중심)
        [Range(0f, 360f)] public float meleeAngleDegrees = 120f;
        // 원거리(구체) 공격력
        [Min(0)] public int rangedAttackDamage;
        // 원거리(구체) 공격 주기 (틱)
        [Min(1)] public int rangedAttackIntervalTicks = 1;
        // 원거리 사거리 R (카메라 가시 영역보다 작게 둔다)
        [Min(0f)] public float rangedRange;
        // 시작 구체 수
        [Min(1)] public int orbBaseCount = 1;
        // 구체가 하나 늘어나는 레벨 간격 (구체 수 = min(시작 수 + 레벨 / 간격, 최대 수))
        [Min(1)] public int orbLevelStep = 5;
        // 최대 구체 수
        [Min(1)] public int orbMaxCount = 5;
        // 구체 회전 반지름 (히어로 중심에서)
        [Min(0f)] public float orbRadius = 1f;
        // 구체 회전 속도 (도 / 틱. 초당 720도 = 14.4)
        public float orbDegreesPerTick = 14.4f;
        // 원거리 투사체 이동 속도 (유닛 / 틱). 0이면 투사체가 날아가지 않는다
        [Min(0.01f)] public float projectileSpeed = 0.4f;
        // 원거리 투사체 명중 판정 반경 (적 반경과 더해 판정)
        [Min(0f)] public float projectileRadius = 0.2f;
        // 접촉 판정 반경 (적 반경과 더해 판정)
        [Min(0f)] public float contactRadius;
        // 피격 후 무적 시간 (틱)
        [Min(0)] public int invincibilityTicks;
        // 사망 후 부활까지 걸리는 시간 (틱)
        [Min(0)] public int respawnTicks;
        // 드롭 획득 반경
        [Min(0f)] public float pickupRadius;
        // Space 상호작용 반경 (공터, 타워 탑승, 코어)
        [Min(0f)] public float interactionRadius;
        // WebGL 빌드용 히어로 View 프리팹 (Poly, View 테이블로 베이킹). Desktop 빌드에서는 빌드 직전에 비워 빌드에 넣지 않는다
        [FormerlySerializedAs("prefab")] public GameObject webPrefab;
        // Desktop 빌드용 히어로 View 프리팹 (PBR, View 테이블로 베이킹). WebGL 빌드에서는 빌드 직전에 비워 빌드에 넣지 않는다
        public GameObject desktopPrefab;
        // 원거리 투사체 View 프리팹 (View 테이블로 베이킹, 없으면 투사체가 보이지 않는다)
        public GameObject projectilePrefab;
        // 구체 View 프리팹 (View 테이블로 베이킹, 없으면 구체가 보이지 않는다)
        public GameObject orbPrefab;
    }
}
