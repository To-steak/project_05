namespace Project05.Foundation
{
    /// <summary>
    /// 한 번 재생하고 풀에 돌아가는 시각 효과 종류. ViewData.visualEffectPrefabs의 인덱스다.
    /// 적 사망 효과는 적 종류마다 다르므로 여기 없고 ViewData.enemyDeathEffectPrefabs에 있다.
    /// 순서를 바꾸면 DefinitionBaker.BakeView의 대응도 함께 확인한다.
    /// </summary>
    public enum VisualEffectKind
    {
        // 적 피격 (EnemyDamagedRecord)
        EnemyHit,
        // 히어로 피격 (HeroDamagedRecord)
        HeroHit,
        // 히어로 근거리 광역 범위 (HeroMeleeAttackRecord, 반지름에 맞춰 크기 조절)
        HeroMeleeRange,
        // 히어로 사망 (HeroDiedRecord)
        HeroDied,
        // 히어로 부활 (HeroRespawnedRecord)
        HeroRespawned,
        // 타워 피격 (TowerDamagedRecord)
        TowerHit,
        // 타워 건설 (TowerBuiltRecord)
        TowerBuilt,
        // 타워 강화 (TowerUpgradedRecord)
        TowerUpgraded,
        // 타워 파괴 (TowerDestroyedRecord)
        TowerDestroyed,
        // 타워 수리 (TowerRepairedRecord)
        TowerRepaired,
        // 코어 피격 (CoreDamagedRecord)
        CoreHit,
    }
}
