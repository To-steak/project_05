using System.Collections.Generic;
using Project05.Component;
using Project05.Component.Data;
using Project05.Component.Entity;
using Project05.Component.Path;
using Project05.Component.Record;
using Project05.Component.Request;
using UnityEngine;

namespace Project05.System
{
    /// <summary>
    /// [Tick 9] 생성 요청 큐를 처리해 적·보스·투사체·드롭·타워를 실제로 만든다. 테이블 조회, 난이도 수식 계산, 엔티티 값 복사를 여기서 한다. 용량이 차서 실패한 요청은 다음 틱에 다시 시도한다.
    /// 구현 브랜치: feat/enemy-wave (적·보스), feat/hero-combat (투사체), feat/drop-progress (드롭), feat/tower (타워)
    /// </summary>
    public static class SpawnSystem
    {
        // 포위 자리를 아직 배정받지 않은 값
        private const int NO_SIEGE_SLOT = -1;

        /// <summary>
        /// 생성 요청 큐를 처리한다. 틱의 마지막에 실행된다.
        /// </summary>
        /// <param name="world">현재 판의 월드</param>
        public static void Tick(World world)
        {
            SpawnEnemies(world);
            SpawnProjectiles(world);
        }

        /// <summary>
        /// 투사체 생성 요청을 처리한다. 발사한 System이 채운 값을 그대로 복사한다.
        /// 저장소가 가득 차면 남은 요청은 큐에 둔 채 다음 틱에 다시 시도한다.
        /// </summary>
        private static void SpawnProjectiles(World world)
        {
            Queue<ProjectileSpawnRequest> requests = world.projectileSpawnRequests;
            while (requests.Count > 0)
            {
                EntityID projectileID;
                if (!world.projectiles.TryCreate(out projectileID))
                {
                    return;
                }

                ProjectileSpawnRequest request = requests.Dequeue();
                ref Projectile projectile = ref world.projectiles.Get(projectileID);
                projectile.previousPosition = request.startPosition;
                projectile.currentPosition = request.startPosition;
                projectile.direction = request.direction;
                projectile.speed = request.speed;
                projectile.radius = request.radius;
                projectile.remainingDistance = request.maxDistance;
                projectile.damage = request.damage;
            }
        }

        /// <summary>
        /// 적 생성 요청을 처리한다. 저장소가 가득 차면 남은 요청은 큐에 둔 채 다음 틱에 다시 시도한다 (순서 유지).
        /// 잘못된 요청(종류·스폰 지점 범위 밖)은 버린다.
        /// </summary>
        private static void SpawnEnemies(World world)
        {
            Queue<EnemySpawnRequest> requests = world.enemySpawnRequests;
            while (requests.Count > 0)
            {
                EnemySpawnRequest request = requests.Peek();
                if (!IsValidEnemyRequest(world, in request))
                {
                    Debug.LogWarning("[SpawnSystem] 잘못된 적 생성 요청을 버린다. 종류 " + request.enemyTypeIndex + ", 스폰 지점 " + request.spawnPointIndex);
                    requests.Dequeue();
                    continue;
                }

                EntityID enemyID;
                if (!world.enemies.TryCreate(out enemyID))
                {
                    return;
                }
                requests.Dequeue();

                ref Enemy enemy = ref world.enemies.Get(enemyID);
                InitializeEnemy(world, in request, ref enemy);

                if (request.isBoss)
                {
                    world.flow.bossID = enemyID;

                    BossSpawnedRecord bossRecord = new BossSpawnedRecord();
                    bossRecord.bossID = enemyID;
                    bossRecord.position = enemy.currentPosition;
                    world.bossSpawnedRecords.Add(bossRecord);
                }
            }
        }

        private static bool IsValidEnemyRequest(World world, in EnemySpawnRequest request)
        {
            return request.enemyTypeIndex >= 0
                && request.enemyTypeIndex < world.enemyStats.Length
                && request.spawnPointIndex >= 0
                && request.spawnPointIndex < world.spawnPoints.Length;
        }

        /// <summary>
        /// 테이블 값에 난이도 수식(DESIGN 3.4.1)을 적용해 적에 복사하고 스폰 지점에 세운다.
        /// HP는 지수 증가, 드롭 골드·XP는 선형 증가, 보스는 배율을 곱한다. 계산은 스폰 시 1회뿐이다.
        /// </summary>
        private static void InitializeEnemy(World world, in EnemySpawnRequest request, ref Enemy enemy)
        {
            ref EnemyStats stats = ref world.enemyStats[request.enemyTypeIndex];
            ref DifficultyStats difficulty = ref world.difficulty;
            ref SpawnPoint spawnPoint = ref world.spawnPoints[request.spawnPointIndex];

            // 1-1(w = 1)에서 기본값 그대로가 되도록 w - 1을 쓴다
            int waveOffset = Mathf.Max(0, request.cumulativeWaveNumber - 1);
            float hpScale = Mathf.Pow(difficulty.hpGrowth, waveOffset);
            float goldScale = 1f + difficulty.goldGrowth * waveOffset;
            float xpScale = 1f + difficulty.xpGrowth * waveOffset;
            if (request.isBoss)
            {
                hpScale *= difficulty.bossHPMultiplier;
                goldScale *= difficulty.bossGoldMultiplier;
                xpScale *= difficulty.bossXPMultiplier;
            }

            enemy.enemyTypeIndex = request.enemyTypeIndex;
            enemy.state = EnemyState.FollowingPath;
            enemy.previousPosition = spawnPoint.position;
            enemy.currentPosition = spawnPoint.position;
            enemy.pathIndex = spawnPoint.pathIndex;
            enemy.nextWaypointIndex = 0;
            // 난이도 계수가 0으로 베이킹돼도 즉사하지 않도록 HP는 최소 1
            enemy.maxHP = Mathf.Max(1, Mathf.RoundToInt(stats.baseHP * hpScale));
            enemy.currentHP = enemy.maxHP;
            enemy.moveSpeed = stats.moveSpeed;
            enemy.radius = stats.radius;
            enemy.contactDamage = stats.contactDamage;
            enemy.coreDamage = stats.coreDamage;
            enemy.towerDamage = stats.towerDamage;
            enemy.attackIntervalTicks = stats.attackIntervalTicks;
            enemy.attackCooldownTicks = 0;
            enemy.targetTowerID = default(EntityID);
            enemy.siegeSlotIndex = NO_SIEGE_SLOT;
            enemy.dropGold = Mathf.RoundToInt(stats.baseDropGold * goldScale);
            enemy.dropXP = Mathf.RoundToInt(stats.baseDropXP * xpScale);
            enemy.isBoss = request.isBoss;
        }
    }
}
