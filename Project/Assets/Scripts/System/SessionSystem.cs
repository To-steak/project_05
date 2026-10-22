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
        /// 막 만든 World를 판 시작 상태(1-1 준비 단계)로 초기화한다.
        /// 베이킹 테이블 참조를 받아두고, 히어로·코어에 테이블 값을 복사한다.
        /// 씬 배치 데이터(경로, 스폰 지점, 공터, 코어 위치, 히어로 시작 위치)는 이보다 먼저 SceneBaker가 채워 둔다.
        /// </summary>
        /// <param name="world">new World()로 만들고 씬 베이킹을 마친 월드</param>
        /// <param name="data">베이킹된 테이블 모음 (Runner가 App.Data를 넘긴다)</param>
        public static void Initialize(World world, GameData data)
        {
            world.tickCount = 0;
            world.pauseReason = PauseReason.None;

            ref GameFlowState flow = ref world.flow;
            flow.phase = GamePhase.Preparing;
            flow.stageNumber = 1;
            flow.waveNumber = 1;
            flow.bossID = default(EntityID);

            // 베이킹 테이블 참조. 이후 테이블 조회는 생성 시점에만 한다
            world.heroStats = data.heroStats;
            world.enemyStats = data.enemyStats;
            world.towerStats = data.towerStats;
            world.waveRule = data.waveRule;
            world.difficulty = data.difficulty;
            world.levelUpOptions = data.levelUpOptions;
            world.progression = data.progression;

            // 첫 웨이브(1-1)도 준비 시간을 거친다
            flow.phaseRemainingTicks = world.waveRule.preparationTicks;

            // 씬 베이킹이 없었어도 순회가 안전하도록 빈 배열로 둔다
            if (world.waypointPaths == null)
            {
                world.waypointPaths = new WaypointPath[0];
            }
            if (world.spawnPoints == null)
            {
                world.spawnPoints = new SpawnPoint[0];
            }
            if (world.towerSlots == null)
            {
                world.towerSlots = new TowerSlot[0];
            }
            world.towers = new EntityStore<Tower>(world.towerSlots.Length);

            InitializeHero(world);

            ref CoreState core = ref world.core;
            core.maxHP = data.coreMaxHP;
            core.currentHP = core.maxHP;
            core.siegeSlotAssignedCount = 0;

            ref ProgressState progress = ref world.progress;
            progress.level = STARTING_LEVEL;
            progress.offeredOptionIndices = new int[ProgressState.OFFERED_OPTION_COUNT];
            progress.goldGainPercent = BASE_GAIN_PERCENT;
            progress.xpGainPercent = BASE_GAIN_PERCENT;

            world.towerUnlock.isUnlockedByTowerType = new bool[world.towerStats.Length];
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

        /// <summary>
        /// 히어로를 시작 위치에 세우고 HeroStats 값을 복사한다.
        /// </summary>
        private static void InitializeHero(World world)
        {
            ref HeroStats heroStats = ref world.heroStats;
            ref Hero hero = ref world.hero;
            hero.state = HeroState.Idle;
            hero.previousPosition = world.heroSpawnPosition;
            hero.currentPosition = world.heroSpawnPosition;
            hero.pathCorners = new Vector3[Hero.MAX_PATH_CORNER_COUNT];
            hero.pathCornerCount = 0;
            hero.nextPathCornerIndex = 0;
            hero.moveSpeed = heroStats.moveSpeed;
            hero.maxHP = heroStats.maxHP;
            hero.currentHP = heroStats.maxHP;
            hero.remainingLives = heroStats.lifeCount;
            hero.attackDamage = heroStats.attackDamage;
            hero.attackIntervalTicks = heroStats.attackIntervalTicks;
            hero.attackCooldownTicks = 0;
            hero.meleeRange = heroStats.meleeRange;
            hero.rangedRange = heroStats.rangedRange;
            hero.projectileSpeed = heroStats.projectileSpeed;
            hero.projectileRadius = heroStats.projectileRadius;
            hero.contactRadius = heroStats.contactRadius;
            hero.invincibilityTicks = heroStats.invincibilityTicks;
            hero.invincibilityRemainingTicks = 0;
            hero.respawnTicks = heroStats.respawnTicks;
            hero.respawnRemainingTicks = 0;
            hero.pickupRadius = heroStats.pickupRadius;
            hero.interactionRadius = heroStats.interactionRadius;
            hero.boardedTowerID = default(EntityID);
        }
    }
}
