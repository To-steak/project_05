using System.Collections.Generic;
using Project05.Component;
using Project05.Component.Entity;
using Project05.Foundation;
using UnityEngine;

namespace Project05.View
{
    /// <summary>
    /// 투사체 동기화(새 ID는 풀에서 꺼내고 사라진 ID는 반납), 위치 보간, 비행 방향 바라보기.
    /// Runner가 LateUpdate에서 정해진 순서로 호출한다. World는 읽기만 한다.
    /// 프리팹(HeroDefinition.projectilePrefab)이 없으면 투사체는 화면에 보이지 않는다.
    /// 구현 브랜치: feat/hero-combat
    /// </summary>
    public static class ProjectileViewSystem
    {
        /// <summary>
        /// 투사체 View를 동기화한다.
        /// </summary>
        /// <param name="world">현재 판의 월드 (읽기 전용)</param>
        /// <param name="viewContext">현재 판의 화면 상태</param>
        public static void Present(World world, ViewContext viewContext)
        {
            ReleaseRemovedViews(world, viewContext);
            SynchronizeAliveProjectiles(world, viewContext);
        }

        /// <summary>
        /// 더 이상 살아있지 않은 투사체의 View를 풀에 반납한다.
        /// </summary>
        private static void ReleaseRemovedViews(World world, ViewContext viewContext)
        {
            Dictionary<EntityID, ProjectileView> projectileViews = viewContext.projectileViews;
            List<EntityID> removedEntityIDs = viewContext.removedEntityIDs;
            removedEntityIDs.Clear();

            foreach (KeyValuePair<EntityID, ProjectileView> viewEntry in projectileViews)
            {
                if (!world.projectiles.IsAlive(viewEntry.Key))
                {
                    removedEntityIDs.Add(viewEntry.Key);
                }
            }

            for (int removedIndex = 0; removedIndex < removedEntityIDs.Count; removedIndex++)
            {
                EntityID projectileID = removedEntityIDs[removedIndex];
                ProjectileView projectileView = projectileViews[projectileID];
                if (projectileView != null)
                {
                    App.Pool.Release(projectileView.gameObject);
                }
                projectileViews.Remove(projectileID);
            }
            removedEntityIDs.Clear();
        }

        /// <summary>
        /// 살아있는 투사체마다 View가 없으면 풀에서 꺼내고, 보간 위치와 비행 방향을 반영한다.
        /// </summary>
        private static void SynchronizeAliveProjectiles(World world, ViewContext viewContext)
        {
            GameObject projectilePrefab = App.ViewData.heroProjectilePrefab;
            if (projectilePrefab == null)
            {
                return;
            }

            EntityStore<Projectile> projectiles = world.projectiles;
            Dictionary<EntityID, ProjectileView> projectileViews = viewContext.projectileViews;
            float alpha = ViewInterpolation.GetAlpha(world);
            int capacity = projectiles.Capacity;

            for (int projectileIndex = 0; projectileIndex < capacity; projectileIndex++)
            {
                if (!projectiles.IsAliveAt(projectileIndex))
                {
                    continue;
                }

                EntityID projectileID = projectiles.GetIDAt(projectileIndex);
                ref readonly Projectile projectile = ref projectiles.GetAt(projectileIndex);

                ProjectileView projectileView;
                if (!projectileViews.TryGetValue(projectileID, out projectileView))
                {
                    projectileView = TakeView(projectilePrefab);
                    if (projectileView == null)
                    {
                        continue;
                    }
                    projectileViews.Add(projectileID, projectileView);
                }

                Vector3 position = Vector3.LerpUnclamped(projectile.previousPosition, projectile.currentPosition, alpha);
                projectileView.transform.SetPositionAndRotation(position, Quaternion.LookRotation(projectile.direction));
            }
        }

        private static ProjectileView TakeView(GameObject projectilePrefab)
        {
            GameObject instance = App.Pool.Get(projectilePrefab);
            ProjectileView projectileView = instance.GetComponent<ProjectileView>();
            if (projectileView == null)
            {
                Debug.LogError("[ProjectileViewSystem] 투사체 프리팹 " + projectilePrefab.name + "에 ProjectileView 컴포넌트가 없다.", projectilePrefab);
                App.Pool.Release(instance);
            }
            return projectileView;
        }
    }
}
