using System.Collections.Generic;
using Project05.Component;
using Project05.Component.Entity;
using Project05.Component.Record;
using Project05.Foundation;
using UnityEngine;

namespace Project05.View
{
    /// <summary>
    /// 적 동기화(새 ID는 풀에서 꺼내고 사라진 ID는 반납), 보간, 피격 플래시, 사망 VFX, 애니메이션.
    /// Runner가 LateUpdate에서 정해진 순서로 호출한다. World는 읽기만 한다.
    /// feat/enemy-wave: 동기화, 위치 보간, 이동 방향 바라보기.
    /// feat/hero-combat: 피격 플래시 (EnemyDamagedRecord).
    /// feat/vfx: 피격 효과(EnemyDamagedRecord), 종류별 사망 효과(EnemyDiedRecord).
    /// feat/animation: 이동 블렌드(Logic 이동 거리 ÷ 이동 속도), 코어·타워 공격(공격 기록, 대상 바라보기),
    /// 사망 애니메이션(사망 기록 → View를 매핑에서 떼어 CorpseTicks 동안 남겼다 반납), 월드 정지 시 애니메이션 정지.
    /// 구현 브랜치: feat/enemy-wave, feat/hero-combat, feat/vfx, feat/animation
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
            bool isStopped = ViewInterpolation.IsSimulationStopped(world);
            // 사망한 적의 View는 반납 전에 매핑에서 떼어 사망 애니메이션을 재생한다
            StartCorpses(world, viewContext);
            ReleaseRemovedViews(world, viewContext);
            SynchronizeAliveEnemies(world, viewContext, isStopped);
            PlayAttacks(world, viewContext);
            StartHitFlashes(world, viewContext);
            // 월드가 멈춘 동안은 플래시를 끄지 않는다 (멈춘 화면에서 색만 바뀌지 않게)
            if (!isStopped)
            {
                EndHitFlashes(viewContext);
            }
            PresentCorpses(viewContext, isStopped);
            PresentEffects(world, viewContext);
        }

        /// <summary>
        /// 사망 기록마다 그 적의 View를 매핑에서 떼어 사망 애니메이션을 재생하고 반납 시각을 기록한다.
        /// Animator가 없는 View(CorpseTicks 0)는 그대로 두어 ReleaseRemovedViews가 바로 반납하게 한다.
        /// </summary>
        private static void StartCorpses(World world, ViewContext viewContext)
        {
            List<EnemyDiedRecord> diedRecords = world.enemyDiedRecords;
            Dictionary<EntityID, EnemyView> enemyViews = viewContext.enemyViews;
            for (int recordIndex = 0; recordIndex < diedRecords.Count; recordIndex++)
            {
                EnemyDiedRecord diedRecord = diedRecords[recordIndex];
                EntityID enemyID = diedRecord.enemyID;
                EnemyView enemyView;
                if (!enemyViews.TryGetValue(enemyID, out enemyView) || enemyView == null || enemyView.CorpseTicks <= 0)
                {
                    continue;
                }

                // 보간 중이던 위치 대신 사망 효과와 같은 기록 위치에 둔다
                enemyView.transform.position = diedRecord.position;

                enemyViews.Remove(enemyID);
                viewContext.enemyHitFlashEndTimes.Remove(enemyID);
                enemyView.SetHitFlash(false);
                enemyView.SetAnimationPaused(false);
                enemyView.PlayDie();

                ActiveEnemyCorpse corpse = new ActiveEnemyCorpse();
                corpse.enemyView = enemyView;
                corpse.releaseTime = Time.time + enemyView.CorpseTicks * Time.fixedDeltaTime;
                viewContext.activeEnemyCorpses.Add(corpse);
            }
        }

        /// <summary>
        /// 월드가 멈춘 동안은 사망 애니메이션을 멈추고 반납 시각을 흐른 시간만큼 미룬다.
        /// 움직이는 동안은 반납 시각이 지난 View를 풀에 돌려준다. 뒤에서부터 돌며 마지막 원소와 바꿔 지운다.
        /// </summary>
        private static void PresentCorpses(ViewContext viewContext, bool isStopped)
        {
            List<ActiveEnemyCorpse> corpses = viewContext.activeEnemyCorpses;
            float now = Time.time;
            float deltaTime = Time.deltaTime;
            for (int corpseIndex = corpses.Count - 1; corpseIndex >= 0; corpseIndex--)
            {
                ActiveEnemyCorpse corpse = corpses[corpseIndex];
                EnemyView enemyView = corpse.enemyView;
                if (enemyView != null)
                {
                    enemyView.SetAnimationPaused(isStopped);
                }

                if (isStopped)
                {
                    corpse.releaseTime += deltaTime;
                    corpses[corpseIndex] = corpse;
                    continue;
                }
                if (enemyView != null && now < corpse.releaseTime)
                {
                    continue;
                }

                if (enemyView != null)
                {
                    App.Pool.Release(enemyView.gameObject);
                }
                int lastIndex = corpses.Count - 1;
                corpses[corpseIndex] = corpses[lastIndex];
                corpses.RemoveAt(lastIndex);
            }
        }

        /// <summary>
        /// 코어·타워 공격 기록마다 그 적의 공격 애니메이션을 재생하고 공격 대상을 바라보게 한다.
        /// 대상 타워가 같은 틱에 사라졌으면 방향은 그대로 둔다.
        /// </summary>
        private static void PlayAttacks(World world, ViewContext viewContext)
        {
            Dictionary<EntityID, EnemyView> enemyViews = viewContext.enemyViews;

            List<EnemyCoreAttackRecord> coreAttackRecords = world.enemyCoreAttackRecords;
            for (int recordIndex = 0; recordIndex < coreAttackRecords.Count; recordIndex++)
            {
                EnemyView enemyView;
                if (enemyViews.TryGetValue(coreAttackRecords[recordIndex].enemyID, out enemyView))
                {
                    FaceTarget(enemyView.transform, world.core.position);
                    enemyView.PlayAttack();
                }
            }

            List<EnemyTowerAttackRecord> towerAttackRecords = world.enemyTowerAttackRecords;
            for (int recordIndex = 0; recordIndex < towerAttackRecords.Count; recordIndex++)
            {
                EnemyTowerAttackRecord towerAttackRecord = towerAttackRecords[recordIndex];
                EnemyView enemyView;
                if (!enemyViews.TryGetValue(towerAttackRecord.enemyID, out enemyView))
                {
                    continue;
                }

                if (world.towers.IsAlive(towerAttackRecord.towerID))
                {
                    FaceTarget(enemyView.transform, world.towers.Get(towerAttackRecord.towerID).position);
                }
                enemyView.PlayAttack();
            }
        }

        /// <summary>
        /// XZ 평면에서 대상 위치를 바라보게 돌린다. 거의 같은 위치면 돌리지 않는다.
        /// </summary>
        private static void FaceTarget(Transform enemyTransform, Vector3 targetPosition)
        {
            Vector3 direction = targetPosition - enemyTransform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude > MIN_FACING_SQR_DISTANCE)
            {
                enemyTransform.rotation = Quaternion.LookRotation(direction);
            }
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
        /// 살아있는 적마다 View가 없으면 풀에서 꺼내고, 보간 위치와 이동 방향, 이동 애니메이션을 반영한다.
        /// 이동 블렌드는 이번 틱 이동 거리를 적의 이동 속도로 나눈 값이다 (DESIGN 5.12: Logic 속도 값).
        /// 프리팹이 없는 종류는 View 없이 건너뛴다 (DefinitionBaker가 경고를 남긴다).
        /// </summary>
        private static void SynchronizeAliveEnemies(World world, ViewContext viewContext, bool isStopped)
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
                float movementSqrDistance = movement.sqrMagnitude;
                if (movementSqrDistance > MIN_FACING_SQR_DISTANCE)
                {
                    enemyTransform.SetPositionAndRotation(position, Quaternion.LookRotation(movement));
                }
                else
                {
                    enemyTransform.position = position;
                }

                enemyView.SetAnimationPaused(isStopped);
                enemyView.SetMoveSpeedRatio(GetSpeedRatio(movementSqrDistance, enemy.moveSpeed));
            }
        }

        /// <summary>
        /// 이번 틱 이동 거리 ÷ 틱당 이동 속도를 0 ~ 1로 돌려준다. 이동 속도가 0이면 0.
        /// </summary>
        private static float GetSpeedRatio(float movementSqrDistance, float moveSpeed)
        {
            if (moveSpeed <= 0f || movementSqrDistance <= MIN_FACING_SQR_DISTANCE)
            {
                return 0f;
            }
            return Mathf.Clamp01(Mathf.Sqrt(movementSqrDistance) / moveSpeed);
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
                return null;
            }

            // 풀에서 다시 꺼낸 View가 이전 적의 사망 자세로 나오지 않게 처음 상태로 되돌린다
            enemyView.ResetAnimation();
            return enemyView;
        }
    }
}
