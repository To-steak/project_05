namespace Project05.Component.Data
{
    /// <summary>
    /// 레벨업 선택지의 효과 종류.
    /// </summary>
    public enum LevelUpEffectKind
    {
        // 히어로 최대 HP 증가 (현재 HP 전부 회복)
        HeroMaxHP,
        // 코어 최대 HP 증가 (현재 HP 전부 회복)
        CoreMaxHP,
        // 히어로 근거리 공격력 증가 (에셋에 정수로 저장되므로 순서를 바꾸지 않는다)
        HeroMeleeAttackDamage,
        // 드롭 골드 획득 퍼센트 증가
        GoldGainPercent,
        // 드롭 XP 획득 퍼센트 증가
        XPGainPercent,
        // 히어로 원거리(구체) 공격력 증가
        HeroRangedAttackDamage,
    }
}
