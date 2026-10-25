namespace Project05.Foundation
{
    /// <summary>
    /// 효과음 종류. AudioData.soundEffectClips의 인덱스다. 대부분 프레임 기록 하나에 대응한다 (DESIGN 5.7).
    /// 순서를 바꾸면 DefinitionBaker.BakeAudio의 대응도 함께 확인한다.
    /// </summary>
    public enum SoundEffectKind
    {
        // 웨이브 시작 (WaveStartedRecord)
        WaveStarted,
        // 웨이브 클리어 (WaveClearedRecord)
        WaveCleared,
        // 보스 등장 (BossSpawnedRecord)
        BossSpawned,
        // 스테이지 클리어 (StageClearedRecord)
        StageCleared,
        // 히어로 근거리 광역 공격 (HeroMeleeAttackRecord)
        HeroMeleeAttack,
        // 히어로 원거리 발사 (HeroRangedShotRecord)
        HeroRangedShot,
        // 히어로 피격 (HeroDamagedRecord)
        HeroDamaged,
        // 히어로 사망 (HeroDiedRecord)
        HeroDied,
        // 히어로 부활 (HeroRespawnedRecord)
        HeroRespawned,
        // 타워 건설 (TowerBuiltRecord)
        TowerBuilt,
        // 타워 강화 (TowerUpgradedRecord)
        TowerUpgraded,
        // 타워 자동 사격 (TowerShotRecord, 탑승 사격 아님)
        TowerShot,
        // 탑승 중 클릭 사격 (TowerShotRecord, 탑승 사격)
        TowerBoardedShot,
        // 타워 피격 (TowerDamagedRecord)
        TowerDamaged,
        // 타워 파괴 (TowerDestroyedRecord)
        TowerDestroyed,
        // 타워 수리 (TowerRepairedRecord)
        TowerRepaired,
        // 적 피격 (EnemyDamagedRecord)
        EnemyDamaged,
        // 일반 적 사망 (EnemyDiedRecord, 보스 아님)
        EnemyDied,
        // 보스 사망 (EnemyDiedRecord, 보스)
        BossDied,
        // 적의 코어 공격 (EnemyCoreAttackRecord)
        EnemyCoreAttack,
        // 적의 타워 공격 (EnemyTowerAttackRecord)
        EnemyTowerAttack,
        // 골드 획득 (DropCollectedRecord, 골드)
        GoldCollected,
        // XP 획득 (DropCollectedRecord, XP)
        XPCollected,
        // 코어 피격 (CoreDamagedRecord)
        CoreDamaged,
        // 명령 거부 (CommandRejectedRecord)
        CommandRejected,
        // 레벨업 (기록 없음, 레벨 값이 오른 것을 읽는다)
        LevelUp,
        // 패배 (기록 없음, 게임 단계가 Defeat가 된 것을 읽는다)
        Defeat,
    }
}
