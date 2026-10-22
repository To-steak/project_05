using UnityEngine;

namespace Project05.Authoring.Definition
{
    /// <summary>
    /// 난이도 증가 계수 (DESIGN 3.4.1). 게임 시작 시 DefinitionBaker가 DifficultyStats로 베이킹한다.
    /// 계산은 SpawnSystem이 적 스폰 시 1회 한다. 계수 값은 밸런싱 때 정한다.
    /// </summary>
    [CreateAssetMenu(fileName = "DifficultyDefinition", menuName = "Project05/Definition/Difficulty")]
    public class DifficultyDefinition : ScriptableObject
    {
        // 적 HP 지수 증가 계수 (baseHP × hpGrowth^(w - 1))
        [Min(1f)] public float hpGrowth = 1f;
        // 드롭 골드 선형 증가 계수 (baseGold × (1 + goldGrowth × (w - 1)))
        [Min(0f)] public float goldGrowth;
        // 드롭 XP 선형 증가 계수 (baseXP × (1 + xpGrowth × (w - 1)))
        [Min(0f)] public float xpGrowth;
        // 보스 HP 배율
        [Min(0f)] public float bossHPMultiplier = 1f;
        // 보스 드롭 골드 배율
        [Min(0f)] public float bossGoldMultiplier = 1f;
        // 보스 드롭 XP 배율
        [Min(0f)] public float bossXPMultiplier = 1f;
    }
}
