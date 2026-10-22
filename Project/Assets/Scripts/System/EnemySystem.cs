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
    /// feat/hero-combat: 피해와 사망(즉시 소멸, 처치 수 증가). 드롭 생성 요청은 feat/drop-progress.
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

                if (enemy.state == EnemyState.FollowingPath)
                {
                    MoveAlongPath(world.waypointPaths, ref enemy);
                }
            }
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
        /// 처치 수로 웨이브를 끝내는 판정, 보스 처치, 점수는 feat/core-siege, 드롭 생성 요청은 feat/drop-progress에서 붙인다.
        /// </summary>
        private static void Kill(World world, EntityID enemyID, ref Enemy enemy)
        {
            EnemyDiedRecord diedRecord = new EnemyDiedRecord();
            diedRecord.enemyID = enemyID;
            diedRecord.enemyTypeIndex = enemy.enemyTypeIndex;
            diedRecord.position = enemy.currentPosition;
            world.enemyDiedRecords.Add(diedRecord);

            world.flow.killCount++;
            world.enemies.Destroy(enemyID);
        }

        /// <summary>
        /// 이번 틱의 이동 거리(moveSpeed)만큼 웨이포인트를 따라간다. 한 틱에 웨이포인트를 여러 개 지날 수 있다.
        /// 마지막 웨이포인트에 닿으면 그 자리에 머문다 (상태 전환은 feat/core-siege).
        /// </summary>
        private static void MoveAlongPath(WaypointPath[] waypointPaths, ref Enemy enemy)
        {
            if (enemy.pathIndex < 0 || enemy.pathIndex >= waypointPaths.Length)
            {
                return;
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
        }
    }
}
