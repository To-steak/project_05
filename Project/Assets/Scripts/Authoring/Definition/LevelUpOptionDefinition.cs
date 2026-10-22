using Project05.Component.Data;
using UnityEngine;

namespace Project05.Authoring.Definition
{
    /// <summary>
    /// 레벨업 선택지 하나. 게임 시작 시 DefinitionBaker가 LevelUpOptionStats(로직)와 표시 이름(View)으로 베이킹한다.
    /// HP·공격력은 고정값을 더하고, 골드·XP 획득량은 퍼센트 포인트를 더한다 (110 = 1.1배, DESIGN 3.6).
    /// </summary>
    [CreateAssetMenu(fileName = "LevelUpOptionDefinition", menuName = "Project05/Definition/Level Up Option")]
    public class LevelUpOptionDefinition : ScriptableObject
    {
        // 효과 종류
        public LevelUpEffectKind effectKind;
        // 증가량. HP·공격력은 고정값, 획득량은 퍼센트 포인트
        [Min(1)] public int amount = 1;
        // 선택지 UI에 보일 이름 (증가량은 UI가 뒤에 붙인다)
        public string displayName;
    }
}
