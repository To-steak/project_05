using System.Collections.Generic;
using Project05.Component;
using Project05.Component.Entity;
using Project05.Component.Record;
using Project05.Foundation;
using UnityEngine;

namespace Project05.View
{
    /// <summary>
    /// 적 동기화(새 ID는 풀에서 꺼내고 사라진 ID는 반납), 보간, 피격 플래시, 사망 VFX.
    /// Runner가 LateUpdate에서 정해진 순서로 호출한다. World는 읽기만 한다.
    /// feat/enemy-wave: 동기화, 위치 보간, 이동 방향 바라보기.
    /// feat/hero-combat: 피격 플래시 (EnemyDamagedRecord).
    /// feat/vfx: 피격 효과(EnemyDamagedRecord), 종류별 사망 효과(EnemyDiedRecord).
    /// 구현 브랜치: feat/enemy-wave, feat/hero-combat, feat/vfx
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
            StartHitFlashes(world, viewContext);
            // 월드가 멈춘 동안은 플래시를 끄지 않는다 (멈춘 화면에서 색만 바뀌지 않게)
            if (!ViewInterpolation.IsSimulationStopped(world))
            {
                EndHitFlashes(viewContext);
            }
            PresentEffects(world, viewContext);
        }

        /// <summary>
        /// 피격 기록마다 피격 효과, 사망 기록마다 그 종류의 사망 효과를 기록 위치에 낸다.
        /// 맞은 틱에 죽은 적도 기록 위치가 있으므로 View 유무와 관계없이 낸다.
        /// </summary>
        private static void PresentEffects(World world, ViewContext viewContext)
        {
            List<EnemyDamagedRecord> damagedRecords = world.enemyDamagedRecords;
            for (int recordIndex = 0; recordIndex < damagedRecords.Count; recordIndex++)
            {
                VisualEffectViewSystem.Spawn(viewContext, VisualEffectKind.EnemyHit, damagedRecords[recordIndex].position, 1f);
            }

            List<EnemyDiedRecord> diedRecords = world.enemyDiedRecords;
            GameObject[] deathEffectPrefabs = App.ViewData.enemyDeathEffectPrefabs;
            for (int recordIndex = 0; recordIndex < diedRecords.Count; recordIndex++)
            {
                EnemyDiedRecord diedRecord = diedRecords[recordIndex];
                if (diedRecord.enemyTypeIndex < 0 || diedRecord.enemyTypeIndex >= deathEffectPrefabs.Length)
                {
                    continue;
                }
                VisualEffectViewSystem.SpawnPrefab(viewContext, deathEffectPrefabs[diedRecord.enemyTypeIndex], diedRecord.position, 1f);
            }
        }

        /// <summary>
        /// 이번 프레임의 피격 기록마다 그 적의 플래시를 켜고 끝나는 시각을 기록한다. 같은 적이 다시 맞으면 시간을 연장한다.
        /// 맞은 틱에 죽은 적은 View가 이미 반납됐으므로 건너뛴다.
        /// </summary>
        private static void StartHitFlashes(World world, ViewContext viewContext)
        {
            List<EnemyDamagedRecord> damagedRecords = world.enemyDamagedRecords;
            for (int recordIndex = 0; recordIndex < damagedRecords.Count; recordIndex++)
            {
                EntityID enemyID = damagedRecords[recordIndex].enemyID;
                EnemyView enemyView;
                if (!viewContext.enemyViews.TryGetValue(enemyID, out enemyView))
                {
                    continue;
                }

                enemyView.SetHitFlash(true);
                viewContext.enemyHitFlashEndTimes[enemyID] = Time.time + enemyView.HitFlashTicks * Time.fixedDeltaTime;
            }
        }

        /// <summary>
        /// 유지 시간이 지난 플래시를 끈다.
        /// </summary>
        private static void EndHitFlashes(ViewContext viewContext)
        {
            Dictionary<EntityID, float> flashEndTimes = viewContext.enemyHitFlashEndTimes;
            List<EntityID> endedEntityIDs = viewContext.removedEntityIDs;
            endedEntityIDs.Clear();

            float now = Time.time;
            foreach (KeyValuePair<EntityID, float> flashEntry in flashEndTimes)
            {
                if (now >= flashEntry.Value)
                {
                    endedEntityIDs.Add(flashEntry.Key);
                }
            }

            for (int endedIndex = 0; endedIndex < endedEntityIDs.Count; endedIndex++)
            {
                EntityID enemyID = endedEntityIDs[endedIndex];
                EnemyView enemyView;
                if (viewContext.enemyViews.TryGetValue(enemyID, out enemyView))
                {
                    enemyView.SetHitFlash(false);
                }
                flashEndTimes.Remove(enemyID);
            }
            endedEntityIDs.Clear();
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
                    // 풀에서 다시 꺼낸 적이 플래시 색으로 나오지 않게 끄고 반납한다
                    enemyView.SetHitFlash(false);
                    App.Pool.Release(enemyView.gameObject);
                }
                enemyViews.Remove(enemyID);
                viewContext.enemyHitFlashEndTimes.Remove(enemyID);
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
