using UnityEngine;

namespace Project05.Authoring.Definition
{
    /// <summary>
    /// 히어로 정의. 게임 시작 시 DefinitionBaker가 HeroStats로 베이킹한다.
    /// 히어로 View는 Game 씬에 직접 배치하므로 프리팹을 두지 않는다.
    /// </summary>
    [CreateAssetMenu(fileName = "HeroDefinition", menuName = "Project05/Hero Definition")]
    public class HeroDefinition : ScriptableObject
    {
        // 최대 체력
        public int maxHP;
        // 시작 목숨 수
        public int lifeCount;
        // 이동 속도 (유닛 / 틱, 0.1 = 초당 5m)
        public float moveSpeed;
        // 공격력 (근거리·원거리 공용, 탑승 시 타워 공격력)
        public int attackDamage;
        // 공격 주기 (틱)
        public int attackIntervalTicks;
        // 근거리 광역 사거리 x
        public float meleeRange;
        // 원거리 사거리 R
        public float rangedRange;
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
