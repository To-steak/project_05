using UnityEngine;

namespace Project05.Authoring.Definition
{
    /// <summary>
    /// 타워 한 종류의 수치와 표시 자료. 게임 시작 시 DefinitionBaker가 TowerStats(로직)와 View 테이블(프리팹, 아이콘, 이름)로 베이킹한다.
    /// 공격은 히어로 원거리와 같은 직선 투사체다 (ProjectileSystem 공용).
    /// </summary>
    [CreateAssetMenu(fileName = "TowerDefinition", menuName = "Project05/Definition/Tower")]
    public class TowerDefinition : ScriptableObject
    {
        // 건설 목록에 보일 이름
        public string displayName;
        // 종류 해금 비용 (골드, 코어 UI)
        [Min(0)] public int unlockCost;
        // 건설 비용 (골드)
        [Min(0)] public int buildCost;
        // 수리 비용 (골드)
        [Min(0)] public int repairCost;
        // 적이 경로를 멈추고 이 타워를 공격하기 시작하는 거리
        [Min(0f)] public float aggroRange;
        // 적이 이 타워를 둘러쌀 때 가장 안쪽 고리 반지름 (고리·자리 간격은 GameDefinition의 코어 포위 값과 같다)
        [Min(0f)] public float siegeFirstRingRadius = 1.5f;
        // 투사체 이동 속도 (유닛 / 틱)
        [Min(0.01f)] public float projectileSpeed = 0.5f;
        // 투사체 명중 판정 반경 (적 반경과 더해 판정)
        [Min(0f)] public float projectileRadius = 0.2f;
        // 탑승 사격 폭발 반경 (명중·소진 지점 중심, 적 중심점 판정. 미탑승 자동 공격은 단일 명중)
        [Min(0f)] public float boardedSplashRadius = 2f;
        // 단계별 수치 (0번이 건설 직후 단계, 강화 2단계면 3개)
        public TowerLevelDefinition[] levels = new TowerLevelDefinition[0];
        // 타워 View 프리팹 (View 테이블로 베이킹)
        public GameObject prefab;
        // 건설 목록 아이콘 (View 테이블로 베이킹)
        public Sprite icon;
    }
}
