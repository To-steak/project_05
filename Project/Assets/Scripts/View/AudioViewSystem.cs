using System.Collections.Generic;
using Project05.Component;
using Project05.Component.Data;
using Project05.Component.Entity;
using Project05.Component.Record;
using Project05.Foundation;

namespace Project05.View
{
    /// <summary>
    /// 배경음 선택과 효과음. 배경음은 게임 단계·보스 생존을 매 프레임 읽어 고르고(지속 상태), 효과음은 프레임 기록을 읽어 낸다 (DESIGN 5.7).
    /// 같은 종류의 효과음은 AudioPlayer가 한 프레임에 한 번만 내므로, 기록이 여러 개여도 종류만 가려 요청한다.
    /// 기록이 없는 레벨업·패배는 ViewContext에 둔 값과 비교해 바뀐 순간에 한 번 낸다.
    /// Runner가 LateUpdate에서 다른 ViewSystem 다음, 프레임 기록을 비우기 전에 호출한다. World는 읽기만 한다.
    /// 구현 브랜치: feat/audio
    /// </summary>
    public static class AudioViewSystem
    {
        /// <summary>
        /// 이번 프레임의 배경음을 고르고 프레임 기록의 효과음을 낸다.
        /// </summary>
        /// <param name="world">현재 판의 월드 (읽기 전용)</param>
        /// <param name="viewContext">현재 판의 화면 상태</param>
        public static void Present(World world, ViewContext viewContext)
        {
            AudioPlayer audioPlayer = App.Audio;
            if (audioPlayer == null)
            {
                return;
            }

            PresentMusic(world, audioPlayer);
            PresentFlowSounds(world, viewContext, audioPlayer);
            PresentHeroSounds(world, audioPlayer);
            PresentTowerSounds(world, audioPlayer);
            PresentEnemySounds(world, audioPlayer);
            PresentDropSounds(world, audioPlayer);
            PlayIfRecorded(world.coreDamagedRecords.Count, SoundEffectKind.CoreDamaged, audioPlayer);
            PlayIfRecorded(world.commandRejectedRecords.Count, SoundEffectKind.CommandRejected, audioPlayer);
        }

        /// <summary>
        /// 패배면 정지, 보스가 살아 있으면 보스 배경음, 아니면 일반 배경음. 같은 배경음이면 AudioPlayer가 다시 틀지 않는다.
        /// </summary>
        private static void PresentMusic(World world, AudioPlayer audioPlayer)
        {
            ref GameFlowState flow = ref world.flow;
            BackgroundMusicKind musicKind;
            if (flow.phase == GamePhase.Defeat)
            {
                musicKind = BackgroundMusicKind.None;
            }
            else if (flow.bossID.IsValid && world.enemies.IsAlive(flow.bossID))
            {
                musicKind = BackgroundMusicKind.Boss;
            }
            else
            {
                musicKind = BackgroundMusicKind.Game;
            }
            audioPlayer.PlayBackgroundMusic(musicKind);
        }

        private static void PresentFlowSounds(World world, ViewContext viewContext, AudioPlayer audioPlayer)
        {
            PlayIfRecorded(world.waveStartedRecords.Count, SoundEffectKind.WaveStarted, audioPlayer);
            PlayIfRecorded(world.waveClearedRecords.Count, SoundEffectKind.WaveCleared, audioPlayer);
            PlayIfRecorded(world.bossSpawnedRecords.Count, SoundEffectKind.BossSpawned, audioPlayer);
            PlayIfRecorded(world.stageClearedRecords.Count, SoundEffectKind.StageCleared, audioPlayer);

            // 레벨업: 판 시작 후 첫 프레임에는 기준만 잡고, 레벨이 오른 프레임에 한 번 낸다
            int level = world.progress.level;
            if (viewContext.hasAudioHeardLevel && level > viewContext.audioHeardLevel)
            {
                audioPlayer.PlaySoundEffect(SoundEffectKind.LevelUp);
            }
            viewContext.audioHeardLevel = level;
            viewContext.hasAudioHeardLevel = true;

            // 패배: 단계가 Defeat가 된 첫 프레임에 한 번
            if (world.flow.phase == GamePhase.Defeat && !viewContext.hasPlayedDefeatSound)
            {
                viewContext.hasPlayedDefeatSound = true;
                audioPlayer.PlaySoundEffect(SoundEffectKind.Defeat);
            }
        }

        private static void PresentHeroSounds(World world, AudioPlayer audioPlayer)
        {
            PlayIfRecorded(world.heroMeleeAttackRecords.Count, SoundEffectKind.HeroMeleeAttack, audioPlayer);
            PlayIfRecorded(world.heroRangedShotRecords.Count, SoundEffectKind.HeroRangedShot, audioPlayer);
            PlayIfRecorded(world.heroDamagedRecords.Count, SoundEffectKind.HeroDamaged, audioPlayer);
            PlayIfRecorded(world.heroDiedRecords.Count, SoundEffectKind.HeroDied, audioPlayer);
            PlayIfRecorded(world.heroRespawnedRecords.Count, SoundEffectKind.HeroRespawned, audioPlayer);
        }

        private static void PresentTowerSounds(World world, AudioPlayer audioPlayer)
        {
            PlayIfRecorded(world.towerBuiltRecords.Count, SoundEffectKind.TowerBuilt, audioPlayer);
            PlayIfRecorded(world.towerUpgradedRecords.Count, SoundEffectKind.TowerUpgraded, audioPlayer);
            PlayIfRecorded(world.towerDamagedRecords.Count, SoundEffectKind.TowerDamaged, audioPlayer);
            PlayIfRecorded(world.towerDestroyedRecords.Count, SoundEffectKind.TowerDestroyed, audioPlayer);
            PlayIfRecorded(world.towerRepairedRecords.Count, SoundEffectKind.TowerRepaired, audioPlayer);
            PlayIfRecorded(world.projectileExplodedRecords.Count, SoundEffectKind.BoardedShotExploded, audioPlayer);

            // 사격은 탑승 사격과 자동 사격을 가른다
            List<TowerShotRecord> towerShotRecords = world.towerShotRecords;
            for (int recordIndex = 0; recordIndex < towerShotRecords.Count; recordIndex++)
            {
                audioPlayer.PlaySoundEffect(towerShotRecords[recordIndex].isBoardedShot ? SoundEffectKind.TowerBoardedShot : SoundEffectKind.TowerShot);
            }
        }

        private static void PresentEnemySounds(World world, AudioPlayer audioPlayer)
        {
            PlayIfRecorded(world.enemyDamagedRecords.Count, SoundEffectKind.EnemyDamaged, audioPlayer);
            PlayIfRecorded(world.enemyCoreAttackRecords.Count, SoundEffectKind.EnemyCoreAttack, audioPlayer);
            PlayIfRecorded(world.enemyTowerAttackRecords.Count, SoundEffectKind.EnemyTowerAttack, audioPlayer);

            // 사망은 보스와 일반 적을 가른다 (적 종류 테이블의 isBoss)
            List<EnemyDiedRecord> enemyDiedRecords = world.enemyDiedRecords;
            EnemyStats[] enemyStats = world.enemyStats;
            for (int recordIndex = 0; recordIndex < enemyDiedRecords.Count; recordIndex++)
            {
                int enemyTypeIndex = enemyDiedRecords[recordIndex].enemyTypeIndex;
                bool isBoss = enemyStats != null && enemyTypeIndex >= 0 && enemyTypeIndex < enemyStats.Length && enemyStats[enemyTypeIndex].isBoss;
                audioPlayer.PlaySoundEffect(isBoss ? SoundEffectKind.BossDied : SoundEffectKind.EnemyDied);
            }
        }

        private static void PresentDropSounds(World world, AudioPlayer audioPlayer)
        {
            List<DropCollectedRecord> dropCollectedRecords = world.dropCollectedRecords;
            for (int recordIndex = 0; recordIndex < dropCollectedRecords.Count; recordIndex++)
            {
                audioPlayer.PlaySoundEffect(dropCollectedRecords[recordIndex].kind == DropKind.Gold ? SoundEffectKind.GoldCollected : SoundEffectKind.XPCollected);
            }
        }

        private static void PlayIfRecorded(int recordCount, SoundEffectKind soundEffectKind, AudioPlayer audioPlayer)
        {
            if (recordCount > 0)
            {
                audioPlayer.PlaySoundEffect(soundEffectKind);
            }
        }
    }
}
