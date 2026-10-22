using System.Collections.Generic;
using Project05.Component;
using Project05.Component.Entity;
using Project05.Foundation;
using UnityEngine;

namespace Project05.View
{
    /// <summary>
    /// 적 동기화(새 ID는 풀에서 꺼내고 사라진 ID는 반납), 보간, 피격 플래시, 사망 VFX.
    /// Runner가 LateUpdate에서 정해진 순서로 호출한다. World는 읽기만 한다.
    /// feat/enemy-wave: 동기화, 위치 보간, 이동 방향 바라보기.
    /// 구현 브랜치: feat/enemy-wave, feat/hero-combat
    /// </summary>
    public static class EnemyViewSystem
    {
        // 이보다 짧은 이동은 방향 전환에 쓰지 않는다 (제자리에서 방향이 튀는 것을 막음)
        private const float MIN_FACING_SQR_DISTANCE = 0.000001f;

        /// <summary>
        /// 적 View를 동기화하고 적 기록을 연출한다.
        /// </summary>
        /// <param name="world">현재 판의 월드 (읽기 전용)</param>
        /// <param name="viewContext">현재 판의 화면 상태</param>
        public static void Present(World world, ViewContext viewContext)
        {
            ReleaseRemovedViews(world, viewContext);
            SynchronizeAliveEnemies(world, viewContext);
        }

        /// <summary>
        /// 더 이상 살아있지 않은 적의 View를 풀에 반납한다. Dictionary 순회 중 삭제를 피하려고 ID를 먼저 모은다.
        /// </summary>
        private static void ReleaseRemovedViews(World world, ViewContext viewContext)
        {
            Dictionary<EntityID, EnemyView> enemyViews = viewContext.enemyViews;
            List<EntityID> removedEntityIDs = viewContext.removedEntityIDs;
            removedEntityIDs.Clear();

            foreach (KeyValuePair<EntityID, EnemyView> viewEntry in enemyViews)
            {
                if (!world.enemies.IsAlive(viewEntry.Key))
                {
                    removedEntityIDs.Add(viewEntry.Key);
                }
            }

            for (int removedIndex = 0; removedIndex < removedEntityIDs.Count; removedIndex++)
            {
                EntityID enemyID = removedEntityIDs[removedIndex];
                EnemyView enemyView = enemyViews[enemyID];
                if (enemyView != null)
                {
                    App.Pool.Release(enemyView.gameObject);
                }
                enemyViews.Remove(enemyID);
            }
            removedEntityIDs.Clear();
        }

        /// <summary>
        /// 살아있는 적마다 View가 없으면 풀에서 꺼내고, 보간 위치와 이동 방향을 반영한다.
        /// 프리팹이 없는 종류는 View 없이 건너뛴다 (DefinitionBaker가 경고를 남긴다).
        /// </summary>
        private static void SynchronizeAliveEnemies(World world, ViewContext viewContext)
        {
            EntityStore<Enemy> enemies = world.enemies;
            Dictionary<EntityID, EnemyView> enemyViews = viewContext.enemyViews;
            float alpha = ViewInterpolation.GetAlpha(world);
            int capacity = enemies.Capacity;

            for (int enemyIndex = 0; enemyIndex < capacity; enemyIndex++)
            {
                if (!enemies.IsAliveAt(enemyIndex))
                {
                    continue;
                }

                EntityID enemyID = enemies.GetIDAt(enemyIndex);
                ref readonly Enemy enemy = ref enemies.GetAt(enemyIndex);

                EnemyView enemyView;
                if (!enemyViews.TryGetValue(enemyID, out enemyView))
                {
                    enemyView = TakeView(enemy.enemyTypeIndex);
                    if (enemyView == null)
                    {
                        continue;
                    }
                    enemyViews.Add(enemyID, enemyView);
                }

                Vector3 position = Vector3.LerpUnclamped(enemy.previousPosition, enemy.currentPosition, alpha);
                Vector3 movement = enemy.currentPosition - enemy.previousPosition;
                movement.y = 0f;

                Transform enemyTransform = enemyView.transform;
                if (movement.sqrMagnitude > MIN_FACING_SQR_DISTANCE)
                {
                    enemyTransform.SetPositionAndRotation(position, Quaternion.LookRotation(movement));
                }
                else
                {
                    enemyTransform.position = position;
                }
            }
        }

        /// <summary>
        /// 적 종류의 프리팹 인스턴스를 풀에서 꺼낸다. 프리팹이 없거나 EnemyView가 없으면 null.
        /// </summary>
        private static EnemyView TakeView(int enemyTypeIndex)
        {
            GameObject[] enemyPrefabs = App.ViewData.enemyPrefabs;
            if (enemyTypeIndex < 0 || enemyTypeIndex >= enemyPrefabs.Length || enemyPrefabs[enemyTypeIndex] == null)
            {
                return null;
            }

            GameObject instance = App.Pool.Get(enemyPrefabs[enemyTypeIndex]);
            EnemyView enemyView = instance.GetComponent<EnemyView>();
            if (enemyView == null)
            {
                Debug.LogError("[EnemyViewSystem] 적 프리팹 " + enemyPrefabs[enemyTypeIndex].name + "에 EnemyView 컴포넌트가 없다.", enemyPrefabs[enemyTypeIndex]);
                App.Pool.Release(instance);
            }
            return enemyView;
        }
    }
}
