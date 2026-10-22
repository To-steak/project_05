using System.Collections.Generic;
using Project05.Component;
using Project05.Component.Entity;
using UnityEngine;

namespace Project05.System
{
    /// <summary>
    /// [Tick 없음] 반경 안 최근접 적, 반경 안 모든 적, 이동 구간에 처음 닿는 적 조회 (히어로·타워 공용). Tick 없이 다른 System이 직접 호출한다.
    /// 거리는 XZ 평면 기준, sqrMagnitude로 비교한다. "이내"는 이하(≤)다 (DESIGN 3.1).
    /// 구현 브랜치: feat/hero-combat
    /// </summary>
    public static class TargetingSystem
    {
        /// <summary>
        /// 중심에서 반경 이내(적 중심 기준)의 가장 가까운 적을 찾는다.
        /// </summary>
        /// <param name="world">현재 판의 월드</param>
        /// <param name="center">조회 중심</param>
        /// <param name="radius">조회 반경</param>
        /// <param name="nearestEnemyID">찾은 적 ID. 없으면 무효 ID</param>
        /// <returns>찾았으면 true</returns>
        public static bool TryFindNearestEnemy(World world, Vector3 center, float radius, out EntityID nearestEnemyID)
        {
            nearestEnemyID = default(EntityID);
            float nearestSqrDistance = radius * radius;
            bool hasFound = false;

            EntityStore<Enemy> enemies = world.enemies;
            int capacity = enemies.Capacity;
            for (int enemyIndex = 0; enemyIndex < capacity; enemyIndex++)
            {
                if (!enemies.IsAliveAt(enemyIndex))
                {
                    continue;
                }

                ref Enemy enemy = ref enemies.GetAt(enemyIndex);
                float sqrDistance = GetSqrDistanceXZ(center, enemy.currentPosition);
                if (sqrDistance <= nearestSqrDistance)
                {
                    nearestSqrDistance = sqrDistance;
                    nearestEnemyID = enemies.GetIDAt(enemyIndex);
                    hasFound = true;
                }
            }

            return hasFound;
        }

        /// <summary>
        /// 중심에서 반경 이내(적 중심 기준)의 모든 적 ID를 결과 목록에 담는다. 목록은 먼저 비운다.
        /// </summary>
        /// <param name="world">현재 판의 월드</param>
        /// <param name="center">조회 중심</param>
        /// <param name="radius">조회 반경</param>
        /// <param name="results">결과를 담을 목록 (보통 world.targetEnemyIDs)</param>
        /// <returns>찾은 적 수</returns>
        public static int CollectEnemiesInRadius(World world, Vector3 center, float radius, List<EntityID> results)
        {
            results.Clear();
            float sqrRadius = radius * radius;

            EntityStore<Enemy> enemies = world.enemies;
            int capacity = enemies.Capacity;
            for (int enemyIndex = 0; enemyIndex < capacity; enemyIndex++)
            {
                if (!enemies.IsAliveAt(enemyIndex))
                {
                    continue;
                }

                ref Enemy enemy = ref enemies.GetAt(enemyIndex);
                if (GetSqrDistanceXZ(center, enemy.currentPosition) <= sqrRadius)
                {
                    results.Add(enemies.GetIDAt(enemyIndex));
                }
            }

            return results.Count;
        }

        /// <summary>
        /// 시작점에서 끝점까지의 이동 구간에 처음 닿는 적을 찾는다 (직선 투사체 명중 판정).
        /// 적 중심과 구간의 최단 거리가 (적 반경 + 투사체 반경) 이하이면 닿은 것으로 보고, 그중 시작점에 가장 가까운 지점에서 닿는 적을 고른다.
        /// </summary>
        /// <param name="world">현재 판의 월드</param>
        /// <param name="start">구간 시작점 (이번 틱 이동 전 위치)</param>
        /// <param name="end">구간 끝점 (이번 틱 이동 후 위치)</param>
        /// <param name="projectileRadius">투사체 명중 판정 반경</param>
        /// <param name="hitEnemyID">처음 닿는 적 ID. 없으면 무효 ID</param>
        /// <returns>닿는 적이 있으면 true</returns>
        public static bool TryFindFirstEnemyOnSegment(World world, Vector3 start, Vector3 end, float projectileRadius, out EntityID hitEnemyID)
        {
            hitEnemyID = default(EntityID);

            float segmentX = end.x - start.x;
            float segmentZ = end.z - start.z;
            float sqrSegmentLength = segmentX * segmentX + segmentZ * segmentZ;
            float bestT = float.MaxValue;

            EntityStore<Enemy> enemies = world.enemies;
            int capacity = enemies.Capacity;
            for (int enemyIndex = 0; enemyIndex < capacity; enemyIndex++)
            {
                if (!enemies.IsAliveAt(enemyIndex))
                {
                    continue;
                }

                ref Enemy enemy = ref enemies.GetAt(enemyIndex);
                float toEnemyX = enemy.currentPosition.x - start.x;
                float toEnemyZ = enemy.currentPosition.z - start.z;

                // 구간 위에서 적 중심에 가장 가까운 지점의 비율 t (0 = 시작점, 1 = 끝점)
                float t = 0f;
                if (sqrSegmentLength > 0f)
                {
                    t = Mathf.Clamp01((toEnemyX * segmentX + toEnemyZ * segmentZ) / sqrSegmentLength);
                }

                float closestX = toEnemyX - segmentX * t;
                float closestZ = toEnemyZ - segmentZ * t;
                float hitRadius = enemy.radius + projectileRadius;
                if (closestX * closestX + closestZ * closestZ > hitRadius * hitRadius)
                {
                    continue;
                }

                if (t < bestT)
                {
                    bestT = t;
                    hitEnemyID = enemies.GetIDAt(enemyIndex);
                }
            }

            return hitEnemyID.IsValid;
        }

        /// <summary>
        /// 두 위치의 XZ 평면 거리 제곱.
        /// </summary>
        /// <param name="from">첫 위치</param>
        /// <param name="to">둘째 위치</param>
        /// <returns>XZ 거리의 제곱</returns>
        public static float GetSqrDistanceXZ(Vector3 from, Vector3 to)
        {
            float deltaX = to.x - from.x;
            float deltaZ = to.z - from.z;
            return deltaX * deltaX + deltaZ * deltaZ;
        }
    }
}
