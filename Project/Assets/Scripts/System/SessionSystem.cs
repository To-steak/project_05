using Project05.Component;
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
        /// 베이킹 테이블·씬 데이터 연결과 히어로·코어 수치 복사는 feat/baking에서 추가한다.
        /// </summary>
        /// <param name="world">new World()로 만든 월드</param>
        public static void Initialize(World world)
        {
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
    }
}
