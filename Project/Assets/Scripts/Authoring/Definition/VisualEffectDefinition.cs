using UnityEngine;

namespace Project05.Authoring.Definition
{
    /// <summary>
    /// 시각 효과 프리팹과 드롭 획득 문구. GameDefinition.visualEffect에 연결하면 게임 시작 시 DefinitionBaker가 View 테이블로 베이킹한다.
    /// 효과 프리팹은 루트에 VisualEffectView, 문구 프리팹은 루트에 FloatingTextView가 있어야 한다. 원점은 바닥(엔티티 위치) 기준이다.
    /// 비워 둔 칸은 효과를 내지 않는다. 적 사망 효과는 적 종류마다 EnemyDefinition.deathEffectPrefab에 둔다.
    /// </summary>
    [CreateAssetMenu(fileName = "VisualEffectDefinition", menuName = "Project05/Definition/Visual Effect")]
    public class VisualEffectDefinition : ScriptableObject
    {
        // ───── 히어로 ─────

        // 히어로 피격
        public GameObject heroHitEffectPrefab;
        // 근거리 광역 범위 표시 (반지름 1 크기로 만든다. 공격 반지름만큼 균일 확대된다)
        public GameObject heroMeleeRangeEffectPrefab;
        // 히어로 사망
        public GameObject heroDiedEffectPrefab;
        // 히어로 부활
        public GameObject heroRespawnedEffectPrefab;

        // ───── 적 ─────

        // 적 피격 (광역 공격이면 맞은 적마다 하나씩 나온다)
        public GameObject enemyHitEffectPrefab;

        // ───── 타워 ─────

        // 타워 피격
        public GameObject towerHitEffectPrefab;
        // 타워 건설
        public GameObject towerBuiltEffectPrefab;
        // 타워 강화
        public GameObject towerUpgradedEffectPrefab;
        // 타워 파괴
        public GameObject towerDestroyedEffectPrefab;
        // 타워 수리
        public GameObject towerRepairedEffectPrefab;

        // ───── 코어 ─────

        // 코어 피격 (화면 경고는 Game 씬의 CoreWarningView가 따로 한다)
        public GameObject coreHitEffectPrefab;

        // ───── 드롭 획득 문구 ─────

        // 골드 획득 문구 프리팹
        public GameObject goldTextPrefab;
        // XP 획득 문구 프리팹
        public GameObject XPTextPrefab;
        // 골드 획득 문구 형식 ({0} = 획득량)
        public string goldTextFormat = "+{0}G";
        // XP 획득 문구 형식 ({0} = 획득량)
        public string XPTextFormat = "+{0}XP";
    }
}
