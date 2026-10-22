using System.Collections.Generic;
using Project05.Component;
using Project05.Component.Entity;
using Project05.Foundation;
using UnityEngine;

namespace Project05.View
{
    /// <summary>
    /// 타워 동기화, 사격·피격·파괴·수리·업그레이드 연출, 탑승 연출.
    /// Runner가 LateUpdate에서 정해진 순서로 호출한다. World는 읽기만 한다.
    /// feat/tower ①: 동기화만. 새 타워 ID는 종류별 프리팹을 풀에서 꺼내 공터 위치에 두고, 사라진 ID는 반납한다.
    /// 구현 브랜치: feat/tower
    /// </summary>
    public static class TowerViewSystem
    {
        /// <summary>
        /// 타워 View를 동기화하고 타워 기록을 연출한다.
        /// </summary>
        /// <param name="world">현재 판의 월드 (읽기 전용)</param>
        /// <param name="viewContext">현재 판의 화면 상태</param>
        public static void Present(World world, ViewContext viewContext)
        {
            ReleaseRemovedViews(world, viewContext);
            TakeNewViews(world, viewContext);
        }

        private static void ReleaseRemovedViews(World world, ViewContext viewContext)
        {
            Dictionary<EntityID, TowerView> towerViews = viewContext.towerViews;
            List<EntityID> removedEntityIDs = viewContext.removedEntityIDs;
            removedEntityIDs.Clear();

            foreach (KeyValuePair<EntityID, TowerView> viewEntry in towerViews)
            {
                if (!world.towers.IsAlive(viewEntry.Key))
                {
                    removedEntityIDs.Add(viewEntry.Key);
                }
            }

            for (int removedIndex = 0; removedIndex < removedEntityIDs.Count; removedIndex++)
            {
                EntityID towerID = removedEntityIDs[removedIndex];
                TowerView towerView = towerViews[towerID];
                if (towerView != null)
                {
                    App.Pool.Release(towerView.gameObject);
                }
                towerViews.Remove(towerID);
            }
            removedEntityIDs.Clear();
        }

        /// <summary>
        /// View가 없는 타워마다 종류에 맞는 프리팹을 풀에서 꺼내 타워 위치에 둔다. 프리팹이 없는 종류는 보이지 않는다.
        /// </summary>
        private static void TakeNewViews(World world, ViewContext viewContext)
        {
            EntityStore<Tower> towers = world.towers;
            Dictionary<EntityID, TowerView> towerViews = viewContext.towerViews;
            GameObject[] towerPrefabs = App.ViewData.towerPrefabs;
            int capacity = towers.Capacity;

            for (int towerIndex = 0; towerIndex < capacity; towerIndex++)
            {
                if (!towers.IsAliveAt(towerIndex))
                {
                    continue;
                }

                EntityID towerID = towers.GetIDAt(towerIndex);
                if (towerViews.ContainsKey(towerID))
                {
                    continue;
                }

                ref readonly Tower tower = ref towers.GetAt(towerIndex);
                if (tower.towerTypeIndex < 0 || tower.towerTypeIndex >= towerPrefabs.Length || towerPrefabs[tower.towerTypeIndex] == null)
                {
                    continue;
                }

                GameObject prefab = towerPrefabs[tower.towerTypeIndex];
                GameObject instance = App.Pool.Get(prefab);
                TowerView towerView = instance.GetComponent<TowerView>();
                if (towerView == null)
                {
                    Debug.LogError("[TowerViewSystem] 타워 프리팹 " + prefab.name + "에 TowerView 컴포넌트가 없다.", prefab);
                    App.Pool.Release(instance);
                    continue;
                }

                towerView.transform.position = tower.position;
                towerViews.Add(towerID, towerView);
            }
        }
    }
}
