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
        // Runner가 시드를 정하기 전의 기본 시드
        private const int DEFAULT_RANDOM_SEED = 0;

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
            InitializeSiegeSlots(world, data);

            ref ProgressState progress = ref world.progress;
            progress.level = STARTING_LEVEL;
            progress.offeredOptionIndices = new int[ProgressState.OFFERED_OPTION_COUNT];
            for (int offeredSlot = 0; offeredSlot < progress.offeredOptionIndices.Length; offeredSlot++)
            {
                progress.offeredOptionIndices[offeredSlot] = ProgressState.NO_OFFERED_OPTION;
            }
            progress.goldGainPercent = BASE_GAIN_PERCENT;
            progress.xpGainPercent = BASE_GAIN_PERCENT;

            world.buildMenuSlotIndex = World.NO_SLOT;

            // 처음에는 아무 타워도 해금돼 있지 않다. 코어 UI에서 골드로 해금하면 건설 목록에 보인다
            world.towerUnlock.isUnlockedByTowerType = new bool[world.towerStats.Length];

            // 시드는 Runner가 InitializeRandom으로 판마다 다시 정한다. 그 전에 쓰여도 null이 아니도록 고정 시드로 만들어 둔다
            InitializeRandom(world, DEFAULT_RANDOM_SEED);
        }

        /// <summary>
        /// 시드 고정 랜덤을 만든다. Initialize 직후 Runner가 판마다 새 시드로 호출한다.
        /// 같은 시드와 같은 입력이면 레벨업 선택지 등 랜덤 결과가 같다.
        /// </summary>
        /// <param name="world">현재 판의 월드</param>
        /// <param name="seed">이번 판의 랜덤 시드</param>
        public static void InitializeRandom(World world, int seed)
        {
            world.random = new global::System.Random(seed);
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
        /// 코어 중심 동심원 위에 포위 자리를 미리 계산하고(SiegeSlotSystem.Layout), 공터마다 타워 포위 자리 배열을 할당한다.
        /// 자리는 적 저장소 용량만큼 만든다 (적이 모두 코어를 포위해도 자리가 모자라지 않는다).
        /// </summary>
        private static void InitializeSiegeSlots(World world, GameData data)
        {
            ref CoreState core = ref world.core;
            core.siegeRingSpacing = data.siegeRingSpacing;
            core.siegeSlotSpacing = data.siegeSlotSpacing;
            core.siegeSlotPositions = new Vector3[World.MAX_ENEMY_COUNT];
            core.siegeSlotRingIndices = new int[World.MAX_ENEMY_COUNT];
            core.isSiegeSlotOccupied = new bool[World.MAX_ENEMY_COUNT];
            SiegeSlotSystem.Layout(core.position, data.siegeFirstRingRadius, core.siegeRingSpacing, core.siegeSlotSpacing, core.siegeSlotPositions, core.siegeSlotRingIndices);

            // 타워 포위 자리 배열은 공터마다 미리 할당한다. 위치는 건설 시 타워 종류의 반지름으로 계산한다
            for (int slotIndex = 0; slotIndex < world.towerSlots.Length; slotIndex++)
            {
                ref TowerSlot towerSlot = ref world.towerSlots[slotIndex];
                towerSlot.siegeSlotPositions = new Vector3[World.TOWER_SIEGE_SLOT_COUNT];
                towerSlot.siegeSlotRingIndices = new int[World.TOWER_SIEGE_SLOT_COUNT];
                towerSlot.isSiegeSlotOccupied = new bool[World.TOWER_SIEGE_SLOT_COUNT];
            }
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
            hero.facingDirection = Vector3.forward;
            hero.pathCorners = new Vector3[Hero.MAX_PATH_CORNER_COUNT];
            hero.pathCornerCount = 0;
            hero.nextPathCornerIndex = 0;
            hero.moveSpeed = heroStats.moveSpeed;
            hero.moveRepathDistance = heroStats.moveRepathDistance;
            hero.moveTargetPosition = world.heroSpawnPosition;
            hero.maxHP = heroStats.maxHP;
            hero.currentHP = heroStats.maxHP;
            hero.remainingLives = heroStats.lifeCount;
            hero.meleeAttackDamage = heroStats.meleeAttackDamage;
            hero.meleeAttackIntervalTicks = heroStats.meleeAttackIntervalTicks;
            hero.meleeAttackCooldownTicks = 0;
            hero.meleeRange = heroStats.meleeRange;
            hero.meleeAngleDegrees = heroStats.meleeAngleDegrees;
            hero.meleeHalfAngleCosine = Mathf.Cos(heroStats.meleeAngleDegrees * 0.5f * Mathf.Deg2Rad);
            hero.rangedAttackDamage = heroStats.rangedAttackDamage;
            hero.rangedAttackIntervalTicks = heroStats.rangedAttackIntervalTicks;
            hero.rangedAttackCooldownTicks = 0;
            hero.rangedRange = heroStats.rangedRange;
            hero.orbBaseCount = heroStats.orbBaseCount;
            hero.orbLevelStep = Mathf.Max(1, heroStats.orbLevelStep);
            hero.orbMaxCount = heroStats.orbMaxCount;
            hero.orbRadius = heroStats.orbRadius;
            hero.orbDegreesPerTick = heroStats.orbDegreesPerTick;
            hero.orbCount = heroStats.orbBaseCount;
            hero.previousOrbAngleDegrees = 0f;
            hero.currentOrbAngleDegrees = 0f;
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
