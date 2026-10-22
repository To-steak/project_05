using UnityEngine;

namespace Project05.Authoring.Definition
{
    /// <summary>
    /// 적 한 종류의 수치와 프리팹. 보스도 이 정의를 쓴다 (isBoss).
    /// 게임 시작 시 DefinitionBaker가 EnemyStats로 베이킹한다. HP·드롭은 난이도 수식 적용 전 기본값이다.
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyDefinition", menuName = "Project05/Definition/Enemy")]
    public class EnemyDefinition : ScriptableObject
    {
        // 기본 체력 (난이도 수식 적용 전)
        [Min(1)] public int baseHP = 1;
        // 이동 속도 (유닛 / 틱)
        [Min(0f)] public float moveSpeed;
        // 접촉 판정 반경 (히어로 반경과 더해 판정)
        [Min(0f)] public float radius;
        // 히어로 접촉 피해
        [Min(0)] public int contactDamage;
        // 코어 공격력
        [Min(0)] public int coreDamage;
        // 타워 공격력
        [Min(0)] public int towerDamage;
        // 코어·타워 공격 주기 (틱)
        [Min(1)] public int attackIntervalTicks = 1;
        // 기본 드롭 골드 (난이도 수식 적용 전)
        [Min(0)] public int baseDropGold;
        // 기본 드롭 XP (난이도 수식 적용 전)
        [Min(0)] public int baseDropXP;
        // 보스 여부
        public bool isBoss;
        // 처치 시 얻는 점수 (임시 규칙: 일반 1, 보스 10. DESIGN 3.4.2)
        [Min(0)] public int killScore = 1;
        // 적 View 프리팹 (View 테이블로 베이킹)
        public GameObject prefab;
        // 사망 VFX 프리팹 (View 테이블로 베이킹, 없으면 null)
        public GameObject deathEffectPrefab;
    }
}
