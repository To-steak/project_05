using UnityEngine;

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
        // 공격력 (근거리·원거리 공용, 탑승 시 타워 공격력)
        [Min(0)] public int attackDamage;
        // 공격 주기 (틱)
        [Min(1)] public int attackIntervalTicks = 1;
        // 근거리 광역 사거리 x
        [Min(0f)] public float meleeRange;
        // 원거리 사거리 R (카메라 가시 영역보다 작게 둔다)
        [Min(0f)] public float rangedRange;
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
        // 히어로 View 프리팹 (View 테이블로 베이킹)
        public GameObject prefab;
        // 원거리 투사체 View 프리팹 (View 테이블로 베이킹, 없으면 투사체가 보이지 않는다)
        public GameObject projectilePrefab;
    }
}
