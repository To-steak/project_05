using UnityEngine;

namespace Project05.Authoring.Definition
{
    /// <summary>
    /// 레벨 진행. 게임 시작 시 DefinitionBaker가 ProgressionStats로 베이킹한다.
    /// </summary>
    [CreateAssetMenu(fileName = "ProgressionDefinition", menuName = "Project05/Definition/Progression")]
    public class ProgressionDefinition : ScriptableObject
    {
        // 레벨별 다음 레벨까지 필요한 XP (인덱스 = 현재 레벨 - 1)
        public int[] requiredXPByLevel = new int[0];
    }
}
