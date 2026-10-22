using Project05.Component;
using Project05.Component.Data;
using Project05.Component.Record;
using Project05.Component.Request;

namespace Project05.System
{
    /// <summary>
    /// [Tick 2] 준비 / 웨이브 / 보스 / 패배 단계를 전이한다. 패배 판정(코어 HP 0, 히어로 목숨 0)을 여기서 한 곳에 모아 한다.
    /// - 준비: 준비 시간이 끝나면 웨이브 시작 (적 수 max = x × 10).
    /// - 웨이브: 처치 수가 max에 닿으면 일반 웨이브는 클리어, 보스 웨이브(n-10)는 보스 생성 요청 후 보스 단계.
    /// - 보스: 보스가 죽으면 스테이지 클리어 → 다음 스테이지 1번 웨이브 준비.
    /// - 패배: 단계를 Defeat로 바꾸면 Runner가 다음 틱부터 모든 System을 멈춘다.
    /// 구현 브랜치: feat/enemy-wave (준비 → 웨이브), feat/core-siege (클리어, 보스, 패배)
    /// </summary>
    public static class GameFlowSystem
    {
        // 요청할 보스 종류가 없을 때의 값
        private const int NO_ENEMY_TYPE = -1;

        /// <summary>
        /// 패배를 먼저 판정하고, 아니면 단계별 전환 조건을 확인한다.
        /// 지난 틱에 생긴 피해·사망·처치를 이번 틱 시작에 판정한다.
        /// </summary>
        /// <param name="world">현재 판의 월드</param>
        public static void Tick(World world)
        {
            if (IsDefeated(world))
            {
                world.flow.phase = GamePhase.Defeat;
                return;
            }

            switch (world.flow.phase)
            {
                case GamePhase.Preparing:
                    TickPreparing(world);
                    break;
                case GamePhase.WaveInProgress:
                    TickWave(world);
                    break;
                case GamePhase.BossInProgress:
                    TickBoss(world);
                    break;
            }
        }

        /// <summary>
        /// 현재 웨이브를 클리어하고 다음 웨이브의 준비 단계로 넘어간다.
        /// 마지막 웨이브(보스 웨이브)를 넘기면 다음 스테이지 1번 웨이브로 간다.
        /// 일반 웨이브 클리어와 디버그 명령(CommandKind.DebugClearWave)이 호출한다.
        /// </summary>
        /// <param name="world">현재 판의 월드</param>
        public static void ClearWave(World world)
        {
            ref GameFlowState flow = ref world.flow;
            if (flow.phase != GamePhase.WaveInProgress && flow.phase != GamePhase.BossInProgress)
            {
                return;
            }

            WaveClearedRecord clearedRecord = new WaveClearedRecord();
            clearedRecord.stageNumber = flow.stageNumber;
            clearedRecord.waveNumber = flow.waveNumber;
            world.waveClearedRecords.Add(clearedRecord);

            flow.waveNumber++;
            if (flow.waveNumber > world.waveRule.bossWaveNumber)
            {
                StageClearedRecord stageRecord = new StageClearedRecord();
                stageRecord.stageNumber = flow.stageNumber;
                world.stageClearedRecords.Add(stageRecord);

                flow.stageNumber++;
                flow.waveNumber = 1;
            }

            flow.bossID = default(EntityID);
            flow.phase = GamePhase.Preparing;
            flow.phaseRemainingTicks = world.waveRule.preparationTicks;
        }

        /// <summary>
        /// 누적 웨이브 번호 w = (n - 1) × 10 + x 를 계산한다 (DESIGN 3.4.1). 스테이지당 웨이브 수는 bossWaveNumber.
        /// </summary>
        /// <param name="world">현재 판의 월드</param>
        /// <returns>1부터 시작하는 누적 웨이브 번호</returns>
        public static int GetCumulativeWaveNumber(World world)
        {
            ref GameFlowState flow = ref world.flow;
            return (flow.stageNumber - 1) * world.waveRule.bossWaveNumber + flow.waveNumber;
        }

        /// <summary>
        /// 패배 조건: 코어 HP 0, 또는 히어로 목숨 0 (DESIGN 3.5.1).
        /// </summary>
        private static bool IsDefeated(World world)
        {
            return world.core.currentHP <= 0 || world.hero.remainingLives <= 0;
        }

        private static void TickPreparing(World world)
        {
            ref GameFlowState flow = ref world.flow;
            if (flow.phaseRemainingTicks > 0)
            {
                flow.phaseRemainingTicks--;
            }

            if (flow.phaseRemainingTicks <= 0)
            {
                StartWave(world);
            }
        }

        /// <summary>
        /// 처치 수가 이번 웨이브 적 수에 닿으면 일반 웨이브는 클리어하고, 보스 웨이브는 보스를 부른다.
        /// </summary>
        private static void TickWave(World world)
        {
            ref GameFlowState flow = ref world.flow;
            if (flow.killCount < flow.waveEnemyCount)
            {
                return;
            }

            if (flow.waveNumber < world.waveRule.bossWaveNumber)
            {
                ClearWave(world);
                return;
            }

            RequestBoss(world);
        }

        /// <summary>
        /// 보스가 생성된 뒤 죽었으면 스테이지를 클리어한다. 생성 전(요청이 큐에 남아 있음)에는 기다린다.
        /// </summary>
        private static void TickBoss(World world)
        {
            ref GameFlowState flow = ref world.flow;
            if (!flow.bossID.IsValid || world.enemies.IsAlive(flow.bossID))
            {
                return;
            }

            ClearWave(world);
        }

        /// <summary>
        /// 보스 생성을 요청하고 보스 단계로 넘어간다. 보스 종류는 isBoss인 첫 종류, 스폰 지점은 일반 적처럼 순서대로 이어서 쓴다.
        /// 보스 종류나 스폰 지점이 없으면 보스 없이 스테이지를 클리어한다.
        /// 보스 ID는 SpawnSystem이 생성하면서 flow.bossID에 넣는다.
        /// </summary>
        private static void RequestBoss(World world)
        {
            ref GameFlowState flow = ref world.flow;
            int bossTypeIndex = FindBossTypeIndex(world.enemyStats);
            if (bossTypeIndex == NO_ENEMY_TYPE || world.spawnPoints.Length == 0)
            {
                ClearWave(world);
                return;
            }

            EnemySpawnRequest request = new EnemySpawnRequest();
            request.enemyTypeIndex = bossTypeIndex;
            request.spawnPointIndex = flow.spawnedEnemyCount % world.spawnPoints.Length;
            request.cumulativeWaveNumber = GetCumulativeWaveNumber(world);
            request.isBoss = true;
            world.enemySpawnRequests.Enqueue(request);

            flow.bossID = default(EntityID);
            flow.phase = GamePhase.BossInProgress;
        }

        private static int FindBossTypeIndex(EnemyStats[] enemyStats)
        {
            for (int enemyTypeIndex = 0; enemyTypeIndex < enemyStats.Length; enemyTypeIndex++)
            {
                if (enemyStats[enemyTypeIndex].isBoss)
                {
                    return enemyTypeIndex;
                }
            }
            return NO_ENEMY_TYPE;
        }

        /// <summary>
        /// 웨이브를 시작한다. 적 수 max = x × enemyCountPerWaveNumber, 처치·스폰 수와 스폰 타이머를 초기화한다.
        /// 첫 적은 다음 WaveSystem 실행(같은 틱)에 바로 요청된다.
        /// </summary>
        private static void StartWave(World world)
        {
            ref GameFlowState flow = ref world.flow;
            flow.phase = GamePhase.WaveInProgress;
            flow.phaseRemainingTicks = 0;
            flow.killCount = 0;
            flow.waveEnemyCount = flow.waveNumber * world.waveRule.enemyCountPerWaveNumber;
            flow.spawnedEnemyCount = 0;
            flow.spawnCooldownTicks = 0;
            flow.bossID = default(EntityID);

            WaveStartedRecord startedRecord = new WaveStartedRecord();
            startedRecord.stageNumber = flow.stageNumber;
            startedRecord.waveNumber = flow.waveNumber;
            world.waveStartedRecords.Add(startedRecord);
        }
    }
}
