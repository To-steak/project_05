namespace Project05.Component.Data
{
    /// <summary>
    /// 레벨업 선택지 하나. LevelUpOptionDefinition에서 베이킹된다.
    /// </summary>
    public struct LevelUpOptionStats
    {
        // 효과 종류
        public LevelUpEffectKind effectKind;
        // 증가량. HP·공격력은 고정값, 획득량은 퍼센트 포인트
        public int amount;
    }
}
