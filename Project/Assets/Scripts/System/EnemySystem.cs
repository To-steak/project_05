using Project05.Component;
using Project05.Component.Entity;
using Project05.Component.Path;
using Project05.Component.Record;
using UnityEngine;

namespace Project05.System
{
    /// <summary>
    /// [Tick 6] 적 경로 이동, aggroRange 안 타워 우선 공격, 코어 포위·공격, 사망 처리(드롭 생성 요청, 처치 수 증가), 타워 파괴(공터 비움, 탑승 중이면 강제 하차)를 처리한다.
    /// feat/enemy-wave: 웨이포인트 경로 이동. 경로 끝에 닿은 적은 마지막 웨이포인트에 멈춰 있다 (코어 포위는 feat/core-siege).
    /// feat/hero-combat: 피해와 사망(즉시 소멸, 처치 수 증가). feat/drop-progress: 사망 시 드롭 생성 요청.
    /// feat/core-siege: 경로 끝에 닿으면 코어 포위 자리를 배정받아 걸어가고, 도착하면 공격 주기마다 코어를 때린다. 죽으면 자리를 비운다.
    /// feat/tower: 경로 위에서 타워 aggroRange에 들어오면 타워 포위 자리로 가서 타워를 때린다. 타워가 파괴되면 경로로 돌아간다.
    /// 구현 브랜치: feat/enemy-wave, feat/hero-combat, feat/core-siege, feat/tower
    /// </summary>
    public static class EnemySystem
    {
        /// <summary>
        /// 모든 적을 한 틱 진행한다.
        /// </summary>
        /// <param name="world">현재 판의 월드</param>
        public static void Tick(World world)
        {
            EntityStore<Enemy> enemies = world.enemies;
            int capacity = enemies.Capacity;
            for (int enemyIndex = 0; enemyIndex < capacity; enemyIndex++)
            {
                if (!enemies.IsAliveAt(enemyIndex))
                {
                    continue;
                }

                ref Enemy enemy = ref enemies.GetAt(enemyIndex);

                // View 보간 기준점. 멈춘 틱에도 갱신해야 이전 이동이 반복 보간되지 않는다
                enemy.previousPosition = enemy.currentPosition;

                switch (enemy.state)
                {
                    case EnemyState.FollowingPath:
                        if (TryBeginTowerSiege(world, ref enemy))
                        {
                            break;
                        }
                        if (MoveAlongPath(world.waypointPaths, ref enemy))
                        {
                            BeginCoreSiege(world, ref enemy);
                        }
                        break;
                    case EnemyState.MovingToSiegeSlot:
                        MoveToSiegeSlot(world, ref enemy);
                        break;
                    case EnemyState.AttackingCore:
                        AttackCore(world, enemies.GetIDAt(enemyIndex), ref enemy);
                        break;
                    case EnemyState.AttackingTower:
                        AttackTower(world, enemies.GetIDAt(enemyIndex), ref enemy);
                        break;
                }
            }
        }

        /// <summary>
        /// 타워를 공격하던(또는 타워 자리로 가던) 적을 경로 추종으로 되돌린다. 타워가 파괴될 때 TowerSystem이 호출한다.
        /// 타워 포위 자리는 타워와 함께 사라지므로 비우지 않는다 (다시 건설할 때 공터의 자리를 모두 비운다).
        /// 경로는 멈췄던 웨이포인트부터 이어서 따라간다.
        /// </summary>
        /// <param name="enemy">되돌릴 적</param>
        public static void ReturnToPath(ref Enemy enemy)
        {
            enemy.state = EnemyState.FollowingPath;
            enemy.targetTowerID = default(EntityID);
            enemy.siegeSlotIndex = SiegeSlotSystem.NO_SIEGE_SLOT;
        }

        /// <summary>
        /// 적에게 피해를 주고 피격 기록을 남긴다. HP가 0 이하가 되면 그 자리에서 사망 처리한다 (슬롯 즉시 비움).
        /// 히어로·투사체·타워가 직접 호출한다. 이미 죽은 ID면 아무것도 하지 않는다.
        /// </summary>
        /// <param name="world">현재 판의 월드</param>
        /// <param name="enemyID">피해를 받을 적</param>
        /// <param name="damage">피해량 (0 이하면 무시)</param>
        /// <returns>이 피해로 적이 죽었으면 true</returns>
        public static bool ApplyDamage(World world, EntityID enemyID, int damage)
        {
            if (damage <= 0 || !world.enemies.IsAlive(enemyID))
            {
                return false;
            }

            ref Enemy enemy = ref world.enemies.Get(enemyID);
            enemy.currentHP -= damage;

            EnemyDamagedRecord damagedRecord = new EnemyDamagedRecord();
            damagedRecord.enemyID = enemyID;
            damagedRecord.position = enemy.currentPosition;
            damagedRecord.damage = damage;
            world.enemyDamagedRecords.Add(damagedRecord);

            if (enemy.currentHP > 0)
            {
                return false;
            }

            Kill(world, enemyID, ref enemy);
            return true;
        }

        /// <summary>
        /// 사망 기록을 남기고 처치 수를 올린 뒤 슬롯을 즉시 비운다. 사망 연출은 ViewSystem이 기록으로 재생한다.
        /// 처치 점수를 더한다. 처치 수로 웨이브를 끝내는 판정과 보스 처치 판정은 GameFlowSystem이 한다. 죽은 자리에 골드·XP 드롭 생성을 요청한다.
        /// </summary>
        private static void Kill(World world, EntityID enemyID, ref Enemy enemy)
        {
            EnemyDiedRecord diedRecord = new EnemyDiedRecord();
            diedRecord.enemyID = enemyID;
            diedRecord.enemyTypeIndex = enemy.enemyTypeIndex;
            diedRecord.position = enemy.currentPosition;
            world.enemyDiedRecords.Add(diedRecord);

            world.flow.killCount++;
            world.progress.score += enemy.killScore;
            DropSystem.RequestSpawn(world, enemy.currentPosition, enemy.dropGold, enemy.dropXP);
            ReleaseSiegeSlot(world, ref enemy);
            world.enemies.Destroy(enemyID);
        }

        /// <summary>
        /// 이번 틱의 이동 거리(moveSpeed)만큼 웨이포인트를 따라간다. 한 틱에 웨이포인트를 여러 개 지날 수 있다.
        /// 마지막 웨이포인트에 닿으면 그 자리에 머문다 (상태 전환은 feat/core-siege).
        /// </summary>
        private static bool MoveAlongPath(WaypointPath[] waypointPaths, ref Enemy enemy)
        {
            if (enemy.pathIndex < 0 || enemy.pathIndex >= waypointPaths.Length)
            {
                return false;
            }

            Vector3[] waypoints = waypointPaths[enemy.pathIndex].waypoints;
            float remainingDistance = enemy.moveSpeed;
            Vector3 position = enemy.currentPosition;

            while (remainingDistance > 0f && enemy.nextWaypointIndex < waypoints.Length)
            {
                ref Vector3 waypoint = ref waypoints[enemy.nextWaypointIndex];
                Vector3 toWaypoint = waypoint - position;
                float sqrDistanceToWaypoint = toWaypoint.sqrMagnitude;

                if (sqrDistanceToWaypoint <= remainingDistance * remainingDistance)
                {
                    position = waypoint;
                    remainingDistance -= Mathf.Sqrt(sqrDistanceToWaypoint);
                    enemy.nextWaypointIndex++;
                }
                else
                {
                    position += toWaypoint * (remainingDistance / Mathf.Sqrt(sqrDistanceToWaypoint));
                    remainingDistance = 0f;
                }
            }

            enemy.currentPosition = position;
            return enemy.nextWaypointIndex >= waypoints.Length;
        }

        /// <summary>
        /// 경로를 따라가던 적이 타워의 aggroRange 안에 들어오면 그 타워 주변 포위 자리를 배정받고 경로를 멈춘다 (DESIGN 3.2).
        /// 적은 경로를 벗어나 타워를 찾아가지 않는다 (경로 위에서 범위 안에 들어온 타워만 본다). 자리가 차 있으면 그 타워를 지나친다.
        /// </summary>
        /// <returns>타워 포위를 시작했으면 true</returns>
        private static bool TryBeginTowerSiege(World world, ref Enemy enemy)
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
                if (TargetingSystem.GetSqrDistanceXZ(enemy.currentPosition, tower.position) > tower.aggroRange * tower.aggroRange)
                {
                    continue;
                }

                ref TowerSlot towerSlot = ref world.towerSlots[tower.slotIndex];
                int siegeSlotIndex = SiegeSlotSystem.TryAssign(towerSlot.siegeSlotPositions, towerSlot.siegeSlotRingIndices, towerSlot.isSiegeSlotOccupied, enemy.currentPosition);
                if (siegeSlotIndex == SiegeSlotSystem.NO_SIEGE_SLOT)
                {
                    continue;
                }

                enemy.targetTowerID = towers.GetIDAt(towerIndex);
                enemy.siegeSlotIndex = siegeSlotIndex;
                enemy.state = EnemyState.MovingToSiegeSlot;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 경로 끝에 닿은 적에게 코어 포위 자리를 배정한다 (SiegeSlotSystem.TryAssign).
        /// 자리 수는 적 저장소 용량과 같아 항상 빈 자리가 있지만, 없으면 경로 끝에 머물며 다음 틱에 다시 시도한다.
        /// </summary>
        private static void BeginCoreSiege(World world, ref Enemy enemy)
        {
            ref CoreState core = ref world.core;
            int siegeSlotIndex = SiegeSlotSystem.TryAssign(core.siegeSlotPositions, core.siegeSlotRingIndices, core.isSiegeSlotOccupied, enemy.currentPosition);
            if (siegeSlotIndex == SiegeSlotSystem.NO_SIEGE_SLOT)
            {
                return;
            }

            enemy.targetTowerID = default(EntityID);
            enemy.siegeSlotIndex = siegeSlotIndex;
            enemy.state = EnemyState.MovingToSiegeSlot;
        }

        /// <summary>
        /// 배정받은 포위 자리로 직선 이동한다. 도착하면 코어(또는 타워) 공격 상태가 되고, 공격 주기 뒤 첫 공격을 한다.
        /// </summary>
        private static void MoveToSiegeSlot(World world, ref Enemy enemy)
        {
            bool isTowerSiege = enemy.targetTowerID.IsValid;
            if (isTowerSiege && !world.towers.IsAlive(enemy.targetTowerID))
            {
                ReturnToPath(ref enemy);
                return;
            }

            Vector3 slotPosition = isTowerSiege
                ? world.towerSlots[world.towers.Get(enemy.targetTowerID).slotIndex].siegeSlotPositions[enemy.siegeSlotIndex]
                : world.core.siegeSlotPositions[enemy.siegeSlotIndex];
            Vector3 toSlot = slotPosition - enemy.currentPosition;
            float sqrDistanceToSlot = toSlot.sqrMagnitude;

            if (sqrDistanceToSlot > enemy.moveSpeed * enemy.moveSpeed)
            {
                enemy.currentPosition += toSlot * (enemy.moveSpeed / Mathf.Sqrt(sqrDistanceToSlot));
                return;
            }

            enemy.currentPosition = slotPosition;
            enemy.state = isTowerSiege ? EnemyState.AttackingTower : EnemyState.AttackingCore;
            enemy.attackCooldownTicks = enemy.attackIntervalTicks;
        }

        /// <summary>
        /// 공격 주기마다 코어에 coreDamage만큼 피해를 준다.
        /// </summary>
        private static void AttackCore(World world, EntityID enemyID, ref Enemy enemy)
        {
            if (enemy.attackCooldownTicks > 0)
            {
                enemy.attackCooldownTicks--;
                if (enemy.attackCooldownTicks > 0)
                {
                    return;
                }
            }

            EnemyCoreAttackRecord attackRecord = new EnemyCoreAttackRecord();
            attackRecord.enemyID = enemyID;
            attackRecord.position = enemy.currentPosition;
            world.enemyCoreAttackRecords.Add(attackRecord);

            CoreSystem.ApplyDamage(world, enemy.coreDamage);
            enemy.attackCooldownTicks = enemy.attackIntervalTicks;
        }

        /// <summary>
        /// 공격 주기마다 대상 타워에 towerDamage만큼 피해를 준다. 타워가 이미 없으면 경로로 돌아간다.
        /// </summary>
        private static void AttackTower(World world, EntityID enemyID, ref Enemy enemy)
        {
            if (!world.towers.IsAlive(enemy.targetTowerID))
            {
                ReturnToPath(ref enemy);
                return;
            }

            if (enemy.attackCooldownTicks > 0)
            {
                enemy.attackCooldownTicks--;
                if (enemy.attackCooldownTicks > 0)
                {
                    return;
                }
            }

            EnemyTowerAttackRecord attackRecord = new EnemyTowerAttackRecord();
            attackRecord.enemyID = enemyID;
            attackRecord.towerID = enemy.targetTowerID;
            attackRecord.position = enemy.currentPosition;
            world.enemyTowerAttackRecords.Add(attackRecord);

            enemy.attackCooldownTicks = enemy.attackIntervalTicks;
            // 이 피해로 타워가 파괴되면 TowerSystem이 이 적을 포함해 공격하던 적들을 경로로 되돌린다
            TowerSystem.ApplyDamage(world, enemy.targetTowerID, enemy.towerDamage);
        }

        /// <summary>
        /// 적이 쓰던 포위 자리(코어 또는 살아있는 대상 타워)를 비워 다른 적이 다시 쓰게 한다.
        /// </summary>
        private static void ReleaseSiegeSlot(World world, ref Enemy enemy)
        {
            if (enemy.siegeSlotIndex == SiegeSlotSystem.NO_SIEGE_SLOT)
            {
                return;
            }

            if (!enemy.targetTowerID.IsValid)
            {
                world.core.isSiegeSlotOccupied[enemy.siegeSlotIndex] = false;
            }
            else if (world.towers.IsAlive(enemy.targetTowerID))
            {
                int slotIndex = world.towers.Get(enemy.targetTowerID).slotIndex;
                world.towerSlots[slotIndex].isSiegeSlotOccupied[enemy.siegeSlotIndex] = false;
            }
            enemy.siegeSlotIndex = SiegeSlotSystem.NO_SIEGE_SLOT;
        }
    }
}
