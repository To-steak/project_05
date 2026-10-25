using System.Collections.Generic;
using Project05.Component;
using Project05.Component.Entity;
using Project05.Component.Record;
using UnityEngine;

namespace Project05.System
{
    /// <summary>
    /// [Tick 5.5] 직선 투사체를 이동시키고 명중을 판정한다. TowerSystem 다음, EnemySystem 앞에 실행한다.
    /// 이번 틱 이동 구간에 처음 닿은 적 하나에게 피해를 주고 사라진다. 최대 비행 거리를 다 쓰면 피해 없이 사라진다.
    /// 폭발 투사체(splashRadius > 0, 탑승 사격)는 명중한 적 위치 또는 소진 지점에서 터져 반경 안 적 전부에게 피해를 준다 (DESIGN 3.2).
    /// 투사체 생성은 SpawnSystem이 한다 (ProjectileSpawnRequest). 히어로 원거리와 타워가 함께 쓴다.
    /// 구현 브랜치: feat/hero-combat, feat/boarded-splash
    /// </summary>
    public static class ProjectileSystem
    {
        /// <summary>
        /// 모든 투사체를 한 틱 진행한다.
        /// </summary>
        /// <param name="world">현재 판의 월드</param>
        public static void Tick(World world)
        {
            EntityStore<Projectile> projectiles = world.projectiles;
            int capacity = projectiles.Capacity;
            for (int projectileIndex = 0; projectileIndex < capacity; projectileIndex++)
            {
                if (!projectiles.IsAliveAt(projectileIndex))
                {
                    continue;
                }

                ref Projectile projectile = ref projectiles.GetAt(projectileIndex);
                projectile.previousPosition = projectile.currentPosition;

                float stepDistance = Mathf.Min(projectile.speed, projectile.remainingDistance);
                Vector3 start = projectile.currentPosition;
                Vector3 end = start + projectile.direction * stepDistance;
                projectile.currentPosition = end;
                projectile.remainingDistance -= stepDistance;

                EntityID hitEnemyID;
                if (TargetingSystem.TryFindFirstEnemyOnSegment(world, start, end, projectile.radius, out hitEnemyID))
                {
                    if (projectile.splashRadius > 0f)
                    {
                        // 명중한 적의 위치에서 터진다 (명중한 적도 반경 안이라 함께 맞는다)
                        Explode(world, world.enemies.Get(hitEnemyID).currentPosition, projectile.splashRadius, projectile.damage);
                    }
                    else
                    {
                        EnemySystem.ApplyDamage(world, hitEnemyID, projectile.damage);
                    }
                    projectiles.Destroy(projectiles.GetIDAt(projectileIndex));
                    continue;
                }

                // 속도가 0이어도 영원히 남지 않도록 거리가 다 됐거나 더 나아갈 수 없으면 사라진다
                if (projectile.remainingDistance <= 0f || stepDistance <= 0f)
                {
                    if (projectile.splashRadius > 0f)
                    {
                        Explode(world, projectile.currentPosition, projectile.splashRadius, projectile.damage);
                    }
                    projectiles.Destroy(projectiles.GetIDAt(projectileIndex));
                }
            }
        }

        /// <summary>
        /// 폭발 중심에서 반경 이내의 적 전부에게 피해를 주고 폭발 기록을 남긴다 (적 중심점 판정).
        /// 피해 도중 적이 죽어 슬롯이 비어도 ID로 조회하므로 안전하다.
        /// </summary>
        private static void Explode(World world, Vector3 center, float radius, int damage)
        {
            List<EntityID> targetEnemyIDs = world.targetEnemyIDs;
            TargetingSystem.CollectEnemiesInRadius(world, center, radius, targetEnemyIDs);
            for (int targetIndex = 0; targetIndex < targetEnemyIDs.Count; targetIndex++)
            {
                EnemySystem.ApplyDamage(world, targetEnemyIDs[targetIndex], damage);
            }
            targetEnemyIDs.Clear();

            ProjectileExplodedRecord explodedRecord = new ProjectileExplodedRecord();
            explodedRecord.position = center;
            explodedRecord.radius = radius;
            world.projectileExplodedRecords.Add(explodedRecord);
        }
    }
}
