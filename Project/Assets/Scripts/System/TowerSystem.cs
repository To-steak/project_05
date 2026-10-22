using Project05.Component;
using Project05.Component.Data;
using Project05.Component.Entity;
using Project05.Component.Record;
using Project05.Component.Request;
using UnityEngine;

namespace Project05.System
{
    /// <summary>
    /// [Tick 5] 미탑승 타워의 자동 공격과 탑승 시 클릭 사격(공격력 = 히어로 공격력)을 처리한다. 업그레이드·수리 처리 함수도 여기에 둔다.
    /// feat/tower ①: 미탑승 자동 공격. 공격 타이머가 준비되면 사거리 안 최근접 적에게 직선 투사체를 쏜다 (히어로 원거리와 같은 ProjectileSystem).
    /// 구현 브랜치: feat/tower (자동 공격 → 탑승 사격·HP·파괴), feat/core-menu (업그레이드·수리)
    /// </summary>
    public static class TowerSystem
    {
        /// <summary>
        /// 모든 타워를 한 틱 진행한다.
        /// </summary>
        /// <param name="world">현재 판의 월드</param>
        public static void Tick(World world)
        {
            EntityStore<Tower> towers = world.towers;
            int capacity = towers.Capacity;
            for (int towerIndex = 0; towerIndex < capacity; towerIndex++)
            {
                if (!towers.IsAliveAt(towerIndex))
                {
                    continue;
                }

                ref Tower tower = ref towers.GetAt(towerIndex);
                if (tower.attackCooldownTicks > 0)
                {
                    tower.attackCooldownTicks--;
                }

                // 탑승 중에는 자동 공격이 꺼지고 클릭 사격만 한다 (feat/tower 후속)
                if (tower.isBoarded || tower.attackCooldownTicks > 0)
                {
                    continue;
                }

                if (TryAutoAttack(world, towers.GetIDAt(towerIndex), ref tower))
                {
                    tower.attackCooldownTicks = tower.attackIntervalTicks;
                }
            }
        }

        /// <summary>
        /// 타워 종류의 해당 단계 수치(최대 HP, 공격력, 사거리, 공격 주기)를 타워에 복사한다. 건설·업그레이드 시점에만 테이블을 본다.
        /// 현재 HP는 바꾸지 않는다 (건설·업그레이드에서 호출하는 쪽이 정한다).
        /// </summary>
        /// <param name="world">현재 판의 월드</param>
        /// <param name="tower">대상 타워</param>
        /// <param name="level">적용할 단계 (0부터, 테이블 범위로 자른다)</param>
        public static void ApplyLevel(World world, ref Tower tower, int level)
        {
            TowerLevelStats[] levels = world.towerStats[tower.towerTypeIndex].levels;
            tower.level = Mathf.Clamp(level, 0, levels.Length - 1);
            ref TowerLevelStats levelStats = ref levels[tower.level];
            tower.maxHP = levelStats.maxHP;
            tower.attackDamage = levelStats.attackDamage;
            tower.range = levelStats.range;
            tower.attackIntervalTicks = levelStats.attackIntervalTicks;
        }

        /// <summary>
        /// 사거리 안 최근접 적 방향으로 직선 투사체 생성을 요청한다. 최대 비행 거리는 사거리다.
        /// </summary>
        private static bool TryAutoAttack(World world, EntityID towerID, ref Tower tower)
        {
            EntityID targetID;
            if (!TargetingSystem.TryFindNearestEnemy(world, tower.position, tower.range, out targetID))
            {
                return false;
            }

            Vector3 targetPosition = world.enemies.Get(targetID).currentPosition;
            Vector3 direction = targetPosition - tower.position;
            direction.y = 0f;
            float sqrDistance = direction.sqrMagnitude;
            direction = sqrDistance > 0f ? direction / Mathf.Sqrt(sqrDistance) : Vector3.forward;

            ProjectileSpawnRequest request = new ProjectileSpawnRequest();
            request.startPosition = tower.position;
            request.direction = direction;
            request.speed = tower.projectileSpeed;
            request.radius = tower.projectileRadius;
            request.maxDistance = tower.range;
            request.damage = tower.attackDamage;
            world.projectileSpawnRequests.Enqueue(request);

            TowerShotRecord shotRecord = new TowerShotRecord();
            shotRecord.towerID = towerID;
            shotRecord.targetPosition = targetPosition;
            shotRecord.isBoardedShot = false;
            world.towerShotRecords.Add(shotRecord);
            return true;
        }
    }
}
