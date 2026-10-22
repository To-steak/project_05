using Project05.Component;
using Project05.Component.Record;

namespace Project05.System
{
    /// <summary>
    /// [Tick 2] 준비 / 웨이브 / 보스 / 패배 단계를 전이한다. 패배 판정(코어 HP 0, 히어로 목숨 0)을 여기서 한 곳에 모아 한다.
    /// feat/enemy-wave: 준비 시간이 끝나면 웨이브 시작, 웨이브 클리어 후 다음 웨이브 준비.
    /// 구현 브랜치: feat/enemy-wave (준비 → 웨이브), feat/core-siege (처치 수 클리어, 보스, 패배)
    /// </summary>
    public static class GameFlowSystem
    {
        /// <summary>
        /// 단계 전환 조건을 확인하고 게임 흐름을 갱신한다.
        /// </summary>
        /// <param name="world">현재 판의 월드</param>
        public static void Tick(World world)
        {
            ref GameFlowState flow = ref world.flow;
            if (flow.phase != GamePhase.Preparing)
            {
                return;
            }

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
        /// 현재 웨이브를 클리어하고 다음 웨이브의 준비 단계로 넘어간다.
        /// 마지막 웨이브(보스 웨이브)를 넘기면 다음 스테이지 1번 웨이브로 간다.
        /// 지금은 디버그 명령(CommandKind.DebugClearWave)만 호출한다. 처치 수 판정과 보스 흐름은 feat/core-siege에서 붙인다.
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
                flow.stageNumber++;
                flow.waveNumber = 1;
            }

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

            WaveStartedRecord startedRecord = new WaveStartedRecord();
            startedRecord.stageNumber = flow.stageNumber;
            startedRecord.waveNumber = flow.waveNumber;
            world.waveStartedRecords.Add(startedRecord);
        }
    }
}
