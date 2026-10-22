using Project05.Component;
using Project05.Component.Data;
using Project05.Component.Entity;
using Project05.Component.Path;
using UnityEngine;

namespace Project05.System
{
    /// <summary>
    /// 판의 수명주기를 다룬다. Tick 없이 Runner가 호출한다.
    /// 판 시작 초기화, 실행 여부 판단, 틱 수 증가, 프레임 기록 비우기.
    /// </summary>
    public static class SessionSystem
    {
        private const int STARTING_LEVEL = 1;
        private const int BASE_GAIN_PERCENT = 100;

        /// <summary>
        /// 막 만든 World를 판 시작 상태(1-1 준비 단계)로 초기화하고 베이킹 테이블 참조를 받는다.
        /// 이후 SceneBaker가 씬 데이터를 쓰고, SessionSystem.Begin이 엔티티를 시작 위치에 놓는다.
        /// </summary>
        /// <param name="world">new World()로 만든 월드</param>
        /// <param name="gameData">베이킹된 테이블 모음 (App.Data)</param>
        public static void Initialize(World world, GameData gameData)
        {
            world.heroStats = gameData.heroStats;
            world.enemyStats = gameData.enemyStats;
            world.towerStats = gameData.towerStats;
            world.waveRule = gameData.waveRule;
            world.difficulty = gameData.difficulty;
            world.levelUpOptions = gameData.levelUpOptions;
            world.progression = gameData.progression;

            world.tickCount = 0;
            world.pauseReason = PauseReason.None;

            ref GameFlowState flow = ref world.flow;
            flow.phase = GamePhase.Preparing;
            flow.stageNumber = 1;
            flow.waveNumber = 1;
            flow.bossID = default(EntityID);

            // 씬 베이킹 전에도 순회가 안전하도록 빈 배열로 둔다
            world.waypointPaths = new WaypointPath[0];
            world.spawnPoints = new SpawnPoint[0];
            world.towerSlots = new TowerSlot[0];
            world.towers = new EntityStore<Tower>(world.towerSlots.Length);

            ref Hero hero = ref world.hero;
            hero.state = HeroState.Idle;
            hero.pathCorners = new Vector3[Hero.MAX_PATH_CORNER_COUNT];
            hero.boardedTowerID = default(EntityID);

            ref ProgressState progress = ref world.progress;
            progress.level = STARTING_LEVEL;
            progress.offeredOptionIndices = new int[ProgressState.OFFERED_OPTION_COUNT];
            progress.goldGainPercent = BASE_GAIN_PERCENT;
            progress.xpGainPercent = BASE_GAIN_PERCENT;

            world.towerUnlock.isUnlockedByTowerType = new bool[0];
        }

        /// <summary>
        /// 씬 베이킹이 끝난 World에서 판을 시작한다. 히어로에 수치를 복사하고 시작 위치에 놓는다.
        /// Runner가 SceneBaker.Bake 직후 호출한다. 코어 설정은 feat/baking·feat/core-siege에서 추가한다.
        /// </summary>
        /// <param name="world">초기화와 씬 베이킹이 끝난 월드</param>
        public static void Begin(World world)
        {
            ref HeroStats heroStats = ref world.heroStats;
            ref Hero hero = ref world.hero;

            hero.moveSpeed = heroStats.moveSpeed;
            hero.maxHP = heroStats.maxHP;
            hero.currentHP = heroStats.maxHP;
            hero.remainingLives = heroStats.lifeCount;
            hero.attackDamage = heroStats.attackDamage;
            hero.attackIntervalTicks = heroStats.attackIntervalTicks;
            hero.attackCooldownTicks = 0;
            hero.meleeRange = heroStats.meleeRange;
            hero.rangedRange = heroStats.rangedRange;
            hero.contactRadius = heroStats.contactRadius;
            hero.invincibilityTicks = heroStats.invincibilityTicks;
            hero.invincibilityRemainingTicks = 0;
            hero.respawnTicks = heroStats.respawnTicks;
            hero.respawnRemainingTicks = 0;
            hero.pickupRadius = heroStats.pickupRadius;
            hero.interactionRadius = heroStats.interactionRadius;

            hero.state = HeroState.Idle;
            hero.previousPosition = world.heroSpawnPosition;
            hero.currentPosition = world.heroSpawnPosition;
            hero.pathCornerCount = 0;
            hero.nextPathCornerIndex = 0;
        }

        /// <summary>
        /// 이번 틱에 CommandSystem을 실행해야 하는지 판단한다. 패배 시에는 CommandSystem도 멈춘다.
        /// </summary>
        /// <param name="world">현재 판의 월드</param>
        /// <returns>CommandSystem을 실행해야 하면 true</returns>
        public static bool IsCommandProcessing(World world)
        {
            return world.flow.phase != GamePhase.Defeat;
        }

        /// <summary>
        /// 이번 틱에 System들을 실행해야 하는지 판단한다. 패배 시 전부 멈추고, 일시정지 중에는 CommandSystem만 실행한다.
        /// </summary>
        /// <param name="world">현재 판의 월드</param>
        /// <returns>CommandSystem 이후의 System을 실행해야 하면 true</returns>
        public static bool IsSimulationRunning(World world)
        {
            return world.flow.phase != GamePhase.Defeat && world.pauseReason == PauseReason.None;
        }

        /// <summary>
        /// 한 틱의 System 실행이 끝났을 때 시뮬레이션 틱 수를 올린다. Runner가 FixedUpdate 마지막에 호출한다.
        /// </summary>
        /// <param name="world">현재 판의 월드</param>
        public static void AdvanceTick(World world)
        {
            world.tickCount++;
        }

        /// <summary>
        /// 모든 프레임 기록 목록을 비운다. Runner가 LateUpdate 마지막(ViewSystem·Presenter 이후)에 호출한다.
        /// 기록 종류를 추가하면 여기에도 추가한다.
        /// </summary>
        /// <param name="world">현재 판의 월드</param>
        public static void ClearFrameRecords(World world)
        {
            world.waveStartedRecords.Clear();
            world.waveClearedRecords.Clear();
            world.bossSpawnedRecords.Clear();
            world.stageClearedRecords.Clear();
            world.heroMeleeAttackRecords.Clear();
            world.heroRangedShotRecords.Clear();
            world.heroDamagedRecords.Clear();
            world.heroDiedRecords.Clear();
            world.heroRespawnedRecords.Clear();
            world.towerBuiltRecords.Clear();
            world.towerUpgradedRecords.Clear();
            world.towerShotRecords.Clear();
            world.towerDamagedRecords.Clear();
            world.towerDestroyedRecords.Clear();
            world.towerRepairedRecords.Clear();
            world.enemyDamagedRecords.Clear();
            world.enemyDiedRecords.Clear();
            world.enemyCoreAttackRecords.Clear();
            world.enemyTowerAttackRecords.Clear();
            world.dropCollectedRecords.Clear();
            world.coreDamagedRecords.Clear();
            world.commandRejectedRecords.Clear();
        }
    }
}
