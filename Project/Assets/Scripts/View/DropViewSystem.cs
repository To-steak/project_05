using System.Collections.Generic;
using Project05.Component;
using Project05.Component.Entity;
using Project05.Component.Record;
using Project05.Foundation;
using UnityEngine;

namespace Project05.View
{
    /// <summary>
    /// 드롭 동기화(새 ID는 풀에서 꺼내고 사라진 ID는 반납). 드롭은 움직이지 않으므로 꺼낼 때 한 번 자리를 잡는다.
    /// 종류(골드 / XP)별 프리팹은 GameDefinition에서 베이킹된다. 프리팹이 없는 종류는 보이지 않는다.
    /// Runner가 LateUpdate에서 정해진 순서로 호출한다. World는 읽기만 한다.
    /// feat/vfx: 획득 문구 "+10G" (DropCollectedRecord). 문구는 획득량별로 ViewContext에 캐시해 같은 양이면 다시 만들지 않는다.
    /// 구현 브랜치: feat/drop-progress, feat/vfx
    /// </summary>
    public static class DropViewSystem
    {
        /// <summary>
        /// 드롭 View를 동기화한다.
        /// </summary>
        /// <param name="world">현재 판의 월드 (읽기 전용)</param>
        /// <param name="viewContext">현재 판의 화면 상태</param>
        public static void Present(World world, ViewContext viewContext)
        {
            ReleaseRemovedViews(world, viewContext);
            TakeNewViews(world, viewContext);
            PresentCollectedTexts(world, viewContext);
        }

        /// <summary>
        /// 획득 기록마다 종류에 맞는 문구 프리팹을 획득 위치에 띄운다.
        /// </summary>
        private static void PresentCollectedTexts(World world, ViewContext viewContext)
        {
            List<DropCollectedRecord> collectedRecords = world.dropCollectedRecords;
            ViewData viewData = App.ViewData;
            for (int recordIndex = 0; recordIndex < collectedRecords.Count; recordIndex++)
            {
                DropCollectedRecord collectedRecord = collectedRecords[recordIndex];
                bool isGold = collectedRecord.kind == DropKind.Gold;
                GameObject prefab = isGold ? viewData.goldTextPrefab : viewData.XPTextPrefab;
                if (prefab == null)
                {
                    continue;
                }

                string text = isGold
                    ? GetAmountText(viewContext.goldTextByAmount, viewData.goldTextFormat, collectedRecord.amount)
                    : GetAmountText(viewContext.XPTextByAmount, viewData.XPTextFormat, collectedRecord.amount);
                VisualEffectViewSystem.SpawnFloatingText(viewContext, prefab, collectedRecord.position, text);
            }
        }

        /// <summary>
        /// 획득량의 문구를 캐시에서 꺼낸다. 처음 보는 양이면 형식으로 만들어 넣는다.
        /// </summary>
        private static string GetAmountText(Dictionary<int, string> textByAmount, string format, int amount)
        {
            string text;
            if (!textByAmount.TryGetValue(amount, out text))
            {
                text = string.Format(format, amount);
                textByAmount.Add(amount, text);
            }
            return text;
        }

        /// <summary>
        /// 더 이상 살아있지 않은(획득된) 드롭의 View를 풀에 반납한다.
        /// </summary>
        private static void ReleaseRemovedViews(World world, ViewContext viewContext)
        {
            Dictionary<EntityID, DropView> dropViews = viewContext.dropViews;
            List<EntityID> removedEntityIDs = viewContext.removedEntityIDs;
            removedEntityIDs.Clear();

            foreach (KeyValuePair<EntityID, DropView> viewEntry in dropViews)
            {
                if (!world.drops.IsAlive(viewEntry.Key))
                {
                    removedEntityIDs.Add(viewEntry.Key);
                }
            }

            for (int removedIndex = 0; removedIndex < removedEntityIDs.Count; removedIndex++)
            {
                EntityID dropID = removedEntityIDs[removedIndex];
                DropView dropView = dropViews[dropID];
                if (dropView != null)
                {
                    App.Pool.Release(dropView.gameObject);
                }
                dropViews.Remove(dropID);
            }
            removedEntityIDs.Clear();
        }

        /// <summary>
        /// View가 없는 드롭마다 종류에 맞는 프리팹을 풀에서 꺼내 드롭 위치에 둔다.
        /// </summary>
        private static void TakeNewViews(World world, ViewContext viewContext)
        {
            EntityStore<Drop> drops = world.drops;
            Dictionary<EntityID, DropView> dropViews = viewContext.dropViews;
            int capacity = drops.Capacity;

            for (int dropIndex = 0; dropIndex < capacity; dropIndex++)
            {
                if (!drops.IsAliveAt(dropIndex))
                {
                    continue;
                }

                EntityID dropID = drops.GetIDAt(dropIndex);
                if (dropViews.ContainsKey(dropID))
                {
                    continue;
                }

                ref readonly Drop drop = ref drops.GetAt(dropIndex);
                GameObject prefab = drop.kind == DropKind.Gold ? App.ViewData.goldDropPrefab : App.ViewData.xpDropPrefab;
                if (prefab == null)
                {
                    continue;
                }

                GameObject instance = App.Pool.Get(prefab);
                DropView dropView = instance.GetComponent<DropView>();
                if (dropView == null)
                {
                    Debug.LogError("[DropViewSystem] 드롭 프리팹 " + prefab.name + "에 DropView 컴포넌트가 없다.", prefab);
                    App.Pool.Release(instance);
                    continue;
                }

                dropView.transform.position = drop.position;
                dropViews.Add(dropID, dropView);
            }
        }
    }
}
