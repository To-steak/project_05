using Project05.Component;
using Project05.Component.Entity;
using UnityEngine;

namespace Project05.System
{
    /// <summary>
    /// [Tick 5.5] 직선 투사체를 이동시키고 명중을 판정한다. TowerSystem 다음, EnemySystem 앞에 실행한다.
    /// 이번 틱 이동 구간에 처음 닿은 적 하나에게 피해를 주고 사라진다. 최대 비행 거리를 다 쓰면 피해 없이 사라진다.
    /// 투사체 생성은 SpawnSystem이 한다 (ProjectileSpawnRequest). 히어로 원거리용이며 이후 타워도 함께 쓴다.
    /// 구현 브랜치: feat/hero-combat
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
                    EnemySystem.ApplyDamage(world, hitEnemyID, projectile.damage);
                    projectiles.Destroy(projectiles.GetIDAt(projectileIndex));
                    continue;
                }

                // 속도가 0이어도 영원히 남지 않도록 거리가 다 됐거나 더 나아갈 수 없으면 사라진다
                if (projectile.remainingDistance <= 0f || stepDistance <= 0f)
                {
                    projectiles.Destroy(projectiles.GetIDAt(projectileIndex));
                }
            }
        }
    }
}
