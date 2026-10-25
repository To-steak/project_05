using System;
using Project05.Authoring.Definition;
using Project05.Component.Data;
using Project05.Foundation;
using UnityEngine;

namespace Project05.Core.Baking
{
    /// <summary>
    /// 게임 시작 시 1회, ScriptableObject 정의(~Definition)를 로직용 struct 테이블(GameData)로 베이킹한다.
    /// 결과는 복사본이므로 Play 중 테이블을 바꿔도 원본 에셋은 바뀌지 않는다.
    /// 정의가 빠져 있으면 경고를 남기고 기본값(0)으로 채워, 어떤 경우에도 null 테이블을 돌려주지 않는다.
    /// View용 테이블(프리팹, VFX)은 BakeView로, 오디오 테이블(클립)은 BakeAudio로 따로 만든다. 항목은 그것을 쓰는 ViewSystem 브랜치에서 추가한다.
    /// </summary>
    public static class DefinitionBaker
    {
        // 획득 문구 형식이 비어 있을 때 쓰는 형식 (획득량 숫자만)
        private const string NUMBER_ONLY_TEXT_FORMAT = "{0}";

        /// <summary>
        /// Resources 폴더에서 루트 정의(GameDefinition)를 불러온다.
        /// </summary>
        /// <returns>루트 정의. 없으면 null</returns>
        public static GameDefinition Load()
        {
            GameDefinition definition = Resources.Load<GameDefinition>(GameDefinition.RESOURCE_PATH);
            if (definition == null)
            {
                Debug.LogWarning("[DefinitionBaker] Resources/" + GameDefinition.RESOURCE_PATH + " 에셋이 없다. 모든 수치를 0으로 베이킹한다.");
            }
            return definition;
        }

        /// <summary>
        /// 루트 정의를 GameData로 베이킹한다. 배열 테이블은 비어 있을 수는 있어도 null이 아니다.
        /// </summary>
        /// <param name="definition">루트 정의 (null이면 기본값 테이블)</param>
        /// <returns>베이킹된 테이블 모음</returns>
        public static GameData Bake(GameDefinition definition)
        {
            GameData data = new GameData();
            data.enemyStats = new EnemyStats[0];
            data.towerStats = new TowerStats[0];
            data.levelUpOptions = new LevelUpOptionStats[0];
            data.progression.requiredXPByLevel = new int[0];

            if (definition == null)
            {
                return data;
            }

            data.coreMaxHP = definition.coreMaxHP;
            data.siegeFirstRingRadius = definition.siegeFirstRingRadius;
            data.siegeRingSpacing = definition.siegeRingSpacing;
            data.siegeSlotSpacing = definition.siegeSlotSpacing;
            data.heroStats = BakeHero(definition.hero, definition);
            data.enemyStats = BakeEnemies(definition.enemies, definition);
            data.waveRule = BakeWaveRule(definition.waveRule, definition);
            data.difficulty = BakeDifficulty(definition.difficulty, definition);
            data.progression = BakeProgression(definition.progression, definition);
            data.levelUpOptions = BakeLevelUpOptions(definition.levelUpOptions, definition);
            data.towerStats = BakeTowers(definition.towers, definition);
            return data;
        }

        /// <summary>
        /// 루트 정의의 프리팹·VFX 참조를 View용 테이블로 베이킹한다. 배열은 비어 있을 수는 있어도 null이 아니다.
        /// 적 프리팹 배열의 인덱스는 GameData.enemyStats와 같다 (빠진 정의는 null 칸으로 자리를 유지).
        /// </summary>
        /// <param name="definition">루트 정의 (null이면 빈 테이블)</param>
        /// <returns>View용 테이블</returns>
        public static ViewData BakeView(GameDefinition definition)
        {
            ViewData viewData = new ViewData();
            if (definition == null)
            {
                return viewData;
            }

            if (definition.hero != null)
            {
                if (definition.hero.projectilePrefab == null)
                {
                    Debug.LogWarning("[DefinitionBaker] " + definition.hero.name + ".projectilePrefab 이(가) 비어 있다. 원거리 투사체가 화면에 보이지 않는다.", definition.hero);
                }
                viewData.heroProjectilePrefab = definition.hero.projectilePrefab;
                if (definition.hero.orbPrefab == null)
                {
                    Debug.LogWarning("[DefinitionBaker] " + definition.hero.name + ".orbPrefab 이(가) 비어 있다. 구체가 화면에 보이지 않는다.", definition.hero);
                }
                viewData.heroOrbPrefab = definition.hero.orbPrefab;
            }

            viewData.goldDropPrefab = definition.goldDropPrefab;
            viewData.xpDropPrefab = definition.xpDropPrefab;
            if (definition.goldDropPrefab == null || definition.xpDropPrefab == null)
            {
                Debug.LogWarning("[DefinitionBaker] " + definition.name + "의 goldDropPrefab 또는 xpDropPrefab이 비어 있다. 그 드롭은 화면에 보이지 않는다.", definition);
            }

            LevelUpOptionDefinition[] optionDefinitions = definition.levelUpOptions;
            int optionCount = optionDefinitions == null ? 0 : optionDefinitions.Length;
            viewData.levelUpOptionNames = new string[optionCount];
            for (int optionIndex = 0; optionIndex < optionCount; optionIndex++)
            {
                LevelUpOptionDefinition optionDefinition = optionDefinitions[optionIndex];
                viewData.levelUpOptionNames[optionIndex] = optionDefinition == null ? string.Empty : optionDefinition.displayName;
            }

            TowerDefinition[] towerDefinitions = definition.towers;
            int towerCount = towerDefinitions == null ? 0 : towerDefinitions.Length;
            viewData.towerPrefabs = new GameObject[towerCount];
            viewData.towerIcons = new Sprite[towerCount];
            viewData.towerNames = new string[towerCount];
            for (int towerTypeIndex = 0; towerTypeIndex < towerCount; towerTypeIndex++)
            {
                TowerDefinition towerDefinition = towerDefinitions[towerTypeIndex];
                if (towerDefinition == null)
                {
                    viewData.towerNames[towerTypeIndex] = string.Empty;
                    continue;
                }
                if (towerDefinition.prefab == null)
                {
                    Debug.LogWarning("[DefinitionBaker] " + towerDefinition.name + ".prefab 이(가) 비어 있다. 이 타워는 화면에 보이지 않는다.", towerDefinition);
                }
                viewData.towerPrefabs[towerTypeIndex] = towerDefinition.prefab;
                viewData.towerIcons[towerTypeIndex] = towerDefinition.icon;
                viewData.towerNames[towerTypeIndex] = towerDefinition.displayName;
            }

            BakeVisualEffects(definition, viewData);

            if (definition.enemies == null)
            {
                return viewData;
            }

            EnemyDefinition[] enemyDefinitions = definition.enemies;
            viewData.enemyPrefabs = new GameObject[enemyDefinitions.Length];
            viewData.enemyDeathEffectPrefabs = new GameObject[enemyDefinitions.Length];
            for (int enemyTypeIndex = 0; enemyTypeIndex < enemyDefinitions.Length; enemyTypeIndex++)
            {
                EnemyDefinition enemyDefinition = enemyDefinitions[enemyTypeIndex];
                if (enemyDefinition == null)
                {
                    continue;
                }
                if (enemyDefinition.prefab == null)
                {
                    Debug.LogWarning("[DefinitionBaker] " + enemyDefinition.name + ".prefab 이(가) 비어 있다. 이 종류의 적은 화면에 보이지 않는다.", enemyDefinition);
                }
                viewData.enemyPrefabs[enemyTypeIndex] = enemyDefinition.prefab;
                viewData.enemyDeathEffectPrefabs[enemyTypeIndex] = enemyDefinition.deathEffectPrefab;
            }
            return viewData;
        }

        /// <summary>
        /// VisualEffectDefinition의 효과 프리팹을 종류 인덱스 배열로, 드롭 획득 문구 프리팹·형식을 그대로 옮긴다.
        /// 정의가 없으면 경고 한 줄만 남기고 비워 둔다 (효과 없이 동작). 형식이 비어 있으면 획득량 숫자만 보인다.
        /// </summary>
        private static void BakeVisualEffects(GameDefinition definition, ViewData viewData)
        {
            VisualEffectDefinition effectDefinition = definition.visualEffect;
            if (effectDefinition == null)
            {
                Debug.LogWarning("[DefinitionBaker] " + definition.name + ".visualEffect 이(가) 비어 있다. 시각 효과가 나오지 않는다.", definition);
                return;
            }

            GameObject[] effectPrefabs = viewData.visualEffectPrefabs;
            effectPrefabs[(int)VisualEffectKind.EnemyHit] = effectDefinition.enemyHitEffectPrefab;
            effectPrefabs[(int)VisualEffectKind.HeroHit] = effectDefinition.heroHitEffectPrefab;
            effectPrefabs[(int)VisualEffectKind.HeroMeleeRange] = effectDefinition.heroMeleeRangeEffectPrefab;
            effectPrefabs[(int)VisualEffectKind.HeroDied] = effectDefinition.heroDiedEffectPrefab;
            effectPrefabs[(int)VisualEffectKind.HeroRespawned] = effectDefinition.heroRespawnedEffectPrefab;
            effectPrefabs[(int)VisualEffectKind.TowerHit] = effectDefinition.towerHitEffectPrefab;
            effectPrefabs[(int)VisualEffectKind.TowerBuilt] = effectDefinition.towerBuiltEffectPrefab;
            effectPrefabs[(int)VisualEffectKind.TowerUpgraded] = effectDefinition.towerUpgradedEffectPrefab;
            effectPrefabs[(int)VisualEffectKind.TowerDestroyed] = effectDefinition.towerDestroyedEffectPrefab;
            effectPrefabs[(int)VisualEffectKind.TowerRepaired] = effectDefinition.towerRepairedEffectPrefab;
            effectPrefabs[(int)VisualEffectKind.CoreHit] = effectDefinition.coreHitEffectPrefab;

            viewData.goldTextPrefab = effectDefinition.goldTextPrefab;
            viewData.XPTextPrefab = effectDefinition.XPTextPrefab;
            viewData.goldTextFormat = string.IsNullOrEmpty(effectDefinition.goldTextFormat) ? NUMBER_ONLY_TEXT_FORMAT : effectDefinition.goldTextFormat;
            viewData.XPTextFormat = string.IsNullOrEmpty(effectDefinition.XPTextFormat) ? NUMBER_ONLY_TEXT_FORMAT : effectDefinition.XPTextFormat;
        }

        /// <summary>
        /// 루트 정의의 AudioDefinition을 오디오 테이블로 베이킹한다. 클립 배열은 항상 종류 enum 길이이며 빈 칸은 null이다.
        /// 보스 배경음·보스 사망·탑승 사격이 비어 있으면 일반 배경음·적 사망·자동 사격 클립으로 채운다.
        /// </summary>
        /// <param name="definition">루트 정의 (null이거나 audio가 비어 있으면 소리 없는 테이블)</param>
        /// <returns>오디오 테이블</returns>
        public static AudioData BakeAudio(GameDefinition definition)
        {
            AudioData audioData = new AudioData();
            if (definition == null)
            {
                return audioData;
            }

            AudioDefinition audioDefinition = definition.audio;
            if (audioDefinition == null)
            {
                Debug.LogWarning("[DefinitionBaker] " + definition.name + ".audio 이(가) 비어 있다. 소리가 나지 않는다.", definition);
                return audioData;
            }

            audioData.musicVolumeScale = audioDefinition.musicVolumeScale;
            audioData.soundEffectVolumeScale = audioDefinition.soundEffectVolumeScale;
            audioData.soundEffectVoiceCount = Mathf.Max(1, audioDefinition.soundEffectVoiceCount);

            AudioClip[] musicClips = audioData.musicClips;
            musicClips[(int)BackgroundMusicKind.MainMenu] = audioDefinition.mainMenuMusic;
            musicClips[(int)BackgroundMusicKind.Game] = audioDefinition.gameMusic;
            musicClips[(int)BackgroundMusicKind.Boss] = audioDefinition.bossMusic != null ? audioDefinition.bossMusic : audioDefinition.gameMusic;

            AudioClip[] soundEffectClips = audioData.soundEffectClips;
            soundEffectClips[(int)SoundEffectKind.WaveStarted] = audioDefinition.waveStartedClip;
            soundEffectClips[(int)SoundEffectKind.WaveCleared] = audioDefinition.waveClearedClip;
            soundEffectClips[(int)SoundEffectKind.BossSpawned] = audioDefinition.bossSpawnedClip;
            soundEffectClips[(int)SoundEffectKind.StageCleared] = audioDefinition.stageClearedClip;
            soundEffectClips[(int)SoundEffectKind.LevelUp] = audioDefinition.levelUpClip;
            soundEffectClips[(int)SoundEffectKind.Defeat] = audioDefinition.defeatClip;
            soundEffectClips[(int)SoundEffectKind.HeroMeleeAttack] = audioDefinition.heroMeleeAttackClip;
            soundEffectClips[(int)SoundEffectKind.HeroRangedShot] = audioDefinition.heroRangedShotClip;
            soundEffectClips[(int)SoundEffectKind.HeroDamaged] = audioDefinition.heroDamagedClip;
            soundEffectClips[(int)SoundEffectKind.HeroDied] = audioDefinition.heroDiedClip;
            soundEffectClips[(int)SoundEffectKind.HeroRespawned] = audioDefinition.heroRespawnedClip;
            soundEffectClips[(int)SoundEffectKind.TowerBuilt] = audioDefinition.towerBuiltClip;
            soundEffectClips[(int)SoundEffectKind.TowerUpgraded] = audioDefinition.towerUpgradedClip;
            soundEffectClips[(int)SoundEffectKind.TowerShot] = audioDefinition.towerShotClip;
            soundEffectClips[(int)SoundEffectKind.TowerBoardedShot] = audioDefinition.towerBoardedShotClip != null ? audioDefinition.towerBoardedShotClip : audioDefinition.towerShotClip;
            soundEffectClips[(int)SoundEffectKind.TowerDamaged] = audioDefinition.towerDamagedClip;
            soundEffectClips[(int)SoundEffectKind.TowerDestroyed] = audioDefinition.towerDestroyedClip;
            soundEffectClips[(int)SoundEffectKind.TowerRepaired] = audioDefinition.towerRepairedClip;
            soundEffectClips[(int)SoundEffectKind.EnemyDamaged] = audioDefinition.enemyDamagedClip;
            soundEffectClips[(int)SoundEffectKind.EnemyDied] = audioDefinition.enemyDiedClip;
            soundEffectClips[(int)SoundEffectKind.BossDied] = audioDefinition.bossDiedClip != null ? audioDefinition.bossDiedClip : audioDefinition.enemyDiedClip;
            soundEffectClips[(int)SoundEffectKind.EnemyCoreAttack] = audioDefinition.enemyCoreAttackClip;
            soundEffectClips[(int)SoundEffectKind.EnemyTowerAttack] = audioDefinition.enemyTowerAttackClip;
            soundEffectClips[(int)SoundEffectKind.GoldCollected] = audioDefinition.goldCollectedClip;
            soundEffectClips[(int)SoundEffectKind.XPCollected] = audioDefinition.XPCollectedClip;
            soundEffectClips[(int)SoundEffectKind.CoreDamaged] = audioDefinition.coreDamagedClip;
            soundEffectClips[(int)SoundEffectKind.CommandRejected] = audioDefinition.commandRejectedClip;
            return audioData;
        }

        private static HeroStats BakeHero(HeroDefinition heroDefinition, GameDefinition owner)
        {
            HeroStats heroStats = new HeroStats();
            if (heroDefinition == null)
            {
                WarnMissing(owner, "hero");
                return heroStats;
            }

            heroStats.maxHP = heroDefinition.maxHP;
            heroStats.lifeCount = heroDefinition.lifeCount;
            heroStats.moveSpeed = heroDefinition.moveSpeed;
            heroStats.moveRepathDistance = heroDefinition.moveRepathDistance;
            heroStats.meleeAttackDamage = heroDefinition.meleeAttackDamage;
            heroStats.meleeAttackIntervalTicks = heroDefinition.meleeAttackIntervalTicks;
            heroStats.meleeRange = heroDefinition.meleeRange;
            heroStats.meleeAngleDegrees = heroDefinition.meleeAngleDegrees;
            heroStats.rangedAttackDamage = heroDefinition.rangedAttackDamage;
            heroStats.rangedAttackIntervalTicks = heroDefinition.rangedAttackIntervalTicks;
            heroStats.rangedRange = heroDefinition.rangedRange;
            heroStats.orbBaseCount = heroDefinition.orbBaseCount;
            heroStats.orbLevelStep = heroDefinition.orbLevelStep;
            heroStats.orbMaxCount = heroDefinition.orbMaxCount;
            heroStats.orbRadius = heroDefinition.orbRadius;
            heroStats.orbDegreesPerTick = heroDefinition.orbDegreesPerTick;
            heroStats.projectileSpeed = heroDefinition.projectileSpeed;
            heroStats.projectileRadius = heroDefinition.projectileRadius;
            heroStats.contactRadius = heroDefinition.contactRadius;
            heroStats.invincibilityTicks = heroDefinition.invincibilityTicks;
            heroStats.respawnTicks = heroDefinition.respawnTicks;
            heroStats.pickupRadius = heroDefinition.pickupRadius;
            heroStats.interactionRadius = heroDefinition.interactionRadius;
            return heroStats;
        }

        private static EnemyStats[] BakeEnemies(EnemyDefinition[] enemyDefinitions, GameDefinition owner)
        {
            if (enemyDefinitions == null)
            {
                WarnMissing(owner, "enemies");
                return new EnemyStats[0];
            }

            // 인덱스가 적 종류이므로 빠진 칸도 자리를 유지한다
            EnemyStats[] enemyStats = new EnemyStats[enemyDefinitions.Length];
            for (int enemyTypeIndex = 0; enemyTypeIndex < enemyDefinitions.Length; enemyTypeIndex++)
            {
                EnemyDefinition enemyDefinition = enemyDefinitions[enemyTypeIndex];
                if (enemyDefinition == null)
                {
                    WarnMissing(owner, "enemies[" + enemyTypeIndex + "]");
                    continue;
                }

                ref EnemyStats stats = ref enemyStats[enemyTypeIndex];
                stats.baseHP = enemyDefinition.baseHP;
                stats.moveSpeed = enemyDefinition.moveSpeed;
                stats.radius = enemyDefinition.radius;
                stats.contactDamage = enemyDefinition.contactDamage;
                stats.coreDamage = enemyDefinition.coreDamage;
                stats.towerDamage = enemyDefinition.towerDamage;
                stats.attackIntervalTicks = enemyDefinition.attackIntervalTicks;
                stats.baseDropGold = enemyDefinition.baseDropGold;
                stats.baseDropXP = enemyDefinition.baseDropXP;
                stats.isBoss = enemyDefinition.isBoss;
                stats.killScore = enemyDefinition.killScore;
            }
            return enemyStats;
        }

        private static WaveRuleStats BakeWaveRule(WaveRuleDefinition waveRuleDefinition, GameDefinition owner)
        {
            WaveRuleStats waveRule = new WaveRuleStats();
            if (waveRuleDefinition == null)
            {
                WarnMissing(owner, "waveRule");
                return waveRule;
            }

            waveRule.enemyCountPerWaveNumber = waveRuleDefinition.enemyCountPerWaveNumber;
            waveRule.spawnIntervalTicks = waveRuleDefinition.spawnIntervalTicks;
            waveRule.preparationTicks = waveRuleDefinition.preparationTicks;
            waveRule.bossWaveNumber = waveRuleDefinition.bossWaveNumber;
            return waveRule;
        }

        private static DifficultyStats BakeDifficulty(DifficultyDefinition difficultyDefinition, GameDefinition owner)
        {
            DifficultyStats difficulty = new DifficultyStats();
            if (difficultyDefinition == null)
            {
                WarnMissing(owner, "difficulty");
                return difficulty;
            }

            difficulty.hpGrowth = difficultyDefinition.hpGrowth;
            difficulty.goldGrowth = difficultyDefinition.goldGrowth;
            difficulty.xpGrowth = difficultyDefinition.xpGrowth;
            difficulty.bossHPMultiplier = difficultyDefinition.bossHPMultiplier;
            difficulty.bossGoldMultiplier = difficultyDefinition.bossGoldMultiplier;
            difficulty.bossXPMultiplier = difficultyDefinition.bossXPMultiplier;
            return difficulty;
        }

        private static TowerStats[] BakeTowers(TowerDefinition[] towerDefinitions, GameDefinition owner)
        {
            if (towerDefinitions == null)
            {
                WarnMissing(owner, "towers");
                return new TowerStats[0];
            }

            // 인덱스가 타워 종류이므로 빠진 칸도 자리를 유지한다 (빠진 칸은 단계 0개라 건설할 수 없다)
            TowerStats[] towerStats = new TowerStats[towerDefinitions.Length];
            for (int towerTypeIndex = 0; towerTypeIndex < towerDefinitions.Length; towerTypeIndex++)
            {
                ref TowerStats stats = ref towerStats[towerTypeIndex];
                stats.levels = new TowerLevelStats[0];

                TowerDefinition towerDefinition = towerDefinitions[towerTypeIndex];
                if (towerDefinition == null)
                {
                    WarnMissing(owner, "towers[" + towerTypeIndex + "]");
                    continue;
                }

                stats.unlockCost = towerDefinition.unlockCost;
                stats.buildCost = towerDefinition.buildCost;
                stats.repairCost = towerDefinition.repairCost;
                stats.aggroRange = towerDefinition.aggroRange;
                stats.siegeFirstRingRadius = towerDefinition.siegeFirstRingRadius;
                stats.projectileSpeed = towerDefinition.projectileSpeed;
                stats.projectileRadius = towerDefinition.projectileRadius;

                TowerLevelDefinition[] levelDefinitions = towerDefinition.levels;
                if (levelDefinitions == null || levelDefinitions.Length == 0)
                {
                    Debug.LogWarning("[DefinitionBaker] " + towerDefinition.name + ".levels 이(가) 비어 있다. 이 타워는 건설할 수 없다.", towerDefinition);
                    continue;
                }

                stats.levels = new TowerLevelStats[levelDefinitions.Length];
                for (int levelIndex = 0; levelIndex < levelDefinitions.Length; levelIndex++)
                {
                    TowerLevelDefinition levelDefinition = levelDefinitions[levelIndex];
                    if (levelDefinition == null)
                    {
                        continue;
                    }

                    ref TowerLevelStats levelStats = ref stats.levels[levelIndex];
                    levelStats.maxHP = levelDefinition.maxHP;
                    levelStats.attackDamage = levelDefinition.attackDamage;
                    levelStats.range = levelDefinition.range;
                    levelStats.attackIntervalTicks = levelDefinition.attackIntervalTicks;
                    levelStats.upgradeCost = levelDefinition.upgradeCost;
                }
            }
            return towerStats;
        }

        private static LevelUpOptionStats[] BakeLevelUpOptions(LevelUpOptionDefinition[] optionDefinitions, GameDefinition owner)
        {
            if (optionDefinitions == null || optionDefinitions.Length == 0)
            {
                WarnMissing(owner, "levelUpOptions");
                return new LevelUpOptionStats[0];
            }

            // 인덱스가 선택지 번호이므로 빠진 칸도 자리를 유지한다 (빠진 칸은 증가량 0)
            LevelUpOptionStats[] options = new LevelUpOptionStats[optionDefinitions.Length];
            for (int optionIndex = 0; optionIndex < optionDefinitions.Length; optionIndex++)
            {
                LevelUpOptionDefinition optionDefinition = optionDefinitions[optionIndex];
                if (optionDefinition == null)
                {
                    WarnMissing(owner, "levelUpOptions[" + optionIndex + "]");
                    continue;
                }

                ref LevelUpOptionStats option = ref options[optionIndex];
                option.effectKind = optionDefinition.effectKind;
                option.amount = optionDefinition.amount;
            }
            return options;
        }

        private static ProgressionStats BakeProgression(ProgressionDefinition progressionDefinition, GameDefinition owner)
        {
            ProgressionStats progression = new ProgressionStats();
            progression.requiredXPByLevel = new int[0];
            if (progressionDefinition == null || progressionDefinition.requiredXPByLevel == null)
            {
                WarnMissing(owner, "progression");
                return progression;
            }

            // 원본 배열을 공유하지 않도록 복사한다
            int[] source = progressionDefinition.requiredXPByLevel;
            progression.requiredXPByLevel = new int[source.Length];
            Array.Copy(source, progression.requiredXPByLevel, source.Length);
            return progression;
        }

        private static void WarnMissing(GameDefinition owner, string fieldName)
        {
            Debug.LogWarning("[DefinitionBaker] " + owner.name + "." + fieldName + " 이(가) 비어 있다. 기본값(0)으로 베이킹한다.", owner);
        }
    }
}
