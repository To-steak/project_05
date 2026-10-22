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
    /// View용 테이블(프리팹, VFX)은 BakeView로 따로 만든다. 항목은 그것을 쓰는 ViewSystem 브랜치에서 추가한다.
    /// </summary>
    public static class DefinitionBaker
    {
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
            data.heroStats = BakeHero(definition.hero, definition);
            data.enemyStats = BakeEnemies(definition.enemies, definition);
            data.waveRule = BakeWaveRule(definition.waveRule, definition);
            data.difficulty = BakeDifficulty(definition.difficulty, definition);
            data.progression = BakeProgression(definition.progression, definition);
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
            if (definition == null || definition.enemies == null)
            {
                return viewData;
            }

            EnemyDefinition[] enemyDefinitions = definition.enemies;
            viewData.enemyPrefabs = new GameObject[enemyDefinitions.Length];
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
            }
            return viewData;
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
            heroStats.attackDamage = heroDefinition.attackDamage;
            heroStats.attackIntervalTicks = heroDefinition.attackIntervalTicks;
            heroStats.meleeRange = heroDefinition.meleeRange;
            heroStats.rangedRange = heroDefinition.rangedRange;
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
