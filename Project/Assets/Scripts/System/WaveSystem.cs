using Project05.Component;
using Project05.Component.Data;
using Project05.Component.Request;

namespace Project05.System
{
    /// <summary>
    /// [Tick 3] 웨이브 스케줄에 따라 스폰 간격마다 적 생성 요청을 넣는다 (웨이브 적 수 max = x × 10).
    /// 임시 규칙 (적 구성 생성 규칙은 미정, DESIGN 3.4.1):
    /// - 적 종류: 보스가 아닌 종류 중 하나를 무작위로 고른다 (보스는 GameFlowSystem이 같은 방식으로 고른다).
    /// - 스폰 갈래: 스폰 지점을 순서대로 돌아가며 쓴다 (0, 1, 2, 0, 1, …).
    /// 구현 브랜치: feat/enemy-wave
    /// </summary>
    public static class WaveSystem
    {
        // 요청할 적 종류가 없을 때의 값
        public const int NO_ENEMY_TYPE = -1;

        /// <summary>
        /// 웨이브 진행 중 스폰 간격이 지났으면 적 생성 요청을 큐에 넣는다. 이번 웨이브 적 수만큼 요청하면 멈춘다.
        /// </summary>
        /// <param name="world">현재 판의 월드</param>
        public static void Tick(World world)
        {
            ref GameFlowState flow = ref world.flow;
            if (flow.phase != GamePhase.WaveInProgress)
            {
                return;
            }
            if (flow.spawnedEnemyCount >= flow.waveEnemyCount)
            {
                return;
            }
            if (world.spawnPoints.Length == 0)
            {
                return;
            }

            if (flow.spawnCooldownTicks > 0)
            {
                flow.spawnCooldownTicks--;
                if (flow.spawnCooldownTicks > 0)
                {
                    return;
                }
            }

            int enemyTypeIndex = PickRandomEnemyTypeIndex(world.enemyStats, false, world.random);
            if (enemyTypeIndex == NO_ENEMY_TYPE)
            {
                return;
            }

            EnemySpawnRequest request = new EnemySpawnRequest();
            request.enemyTypeIndex = enemyTypeIndex;
            request.spawnPointIndex = flow.spawnedEnemyCount % world.spawnPoints.Length;
            request.cumulativeWaveNumber = GameFlowSystem.GetCumulativeWaveNumber(world);
            request.isBoss = false;
            world.enemySpawnRequests.Enqueue(request);

            flow.spawnedEnemyCount++;
            flow.spawnCooldownTicks = world.waveRule.spawnIntervalTicks;
        }

        /// <summary>
        /// 임시 규칙: 일반 적(또는 보스) 종류 중 하나를 같은 확률로 고른다. 적 구성 생성 규칙이 정해지면 바꾼다.
        /// 시드 고정 랜덤(world.random)을 써서 같은 시드면 같은 순서로 나온다. 배열을 새로 만들지 않는다.
        /// </summary>
        /// <param name="enemyStats">적 종류 테이블</param>
        /// <param name="isBoss">보스 중에서 고르면 true, 일반 적 중에서 고르면 false</param>
        /// <param name="random">시드 고정 랜덤</param>
        /// <returns>고른 적 종류 인덱스. 해당 종류가 하나도 없으면 NO_ENEMY_TYPE</returns>
        public static int PickRandomEnemyTypeIndex(EnemyStats[] enemyStats, bool isBoss, global::System.Random random)
        {
            int candidateCount = 0;
            for (int enemyTypeIndex = 0; enemyTypeIndex < enemyStats.Length; enemyTypeIndex++)
            {
                if (enemyStats[enemyTypeIndex].isBoss == isBoss)
                {
                    candidateCount++;
                }
            }
            if (candidateCount == 0)
            {
                return NO_ENEMY_TYPE;
            }

            int remainingOrder = random.Next(candidateCount);
            for (int enemyTypeIndex = 0; enemyTypeIndex < enemyStats.Length; enemyTypeIndex++)
            {
                if (enemyStats[enemyTypeIndex].isBoss != isBoss)
                {
                    continue;
                }
                if (remainingOrder == 0)
                {
                    return enemyTypeIndex;
                }
                remainingOrder--;
            }
            return NO_ENEMY_TYPE;
        }
    }
}