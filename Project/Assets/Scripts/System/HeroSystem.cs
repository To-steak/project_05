using System.Collections.Generic;
using Project05.Component;
using Project05.Component.Entity;
using Project05.Component.Record;
using Project05.Component.Request;
using UnityEngine;

namespace Project05.System
{
    /// <summary>
    /// [Tick 4] 히어로 이동, 탑승/하차 상태, 공격 타이머 1개로 근거리 광역 / 원거리 자동 공격, 접촉 피해와 무적, 사망·목숨·부활을 처리한다.
    /// 구현 브랜치: feat/hero-movement, feat/hero-combat, feat/core-siege
    /// </summary>
    public static class HeroSystem
    {
        /// <summary>
        /// 히어로를 한 틱 진행한다.
        /// </summary>
        /// <param name="world">현재 판의 월드</param>
        public static void Tick(World world)
        {
            ref Hero hero = ref world.hero;

            // View 보간 기준점. 움직이지 않는 틱에도 갱신해야 이전 이동이 반복 보간되지 않는다
            hero.previousPosition = hero.currentPosition;

            if (hero.state == HeroState.Moving)
            {
                MoveAlongPath(ref hero);
            }

            TickAutoAttack(world);
        }

        /// <summary>
        /// 공격 타이머 하나로 자동 공격한다 (DESIGN 3.1). 타이머가 준비된 틱에 상황을 보고 공격을 하나 고른다.
        /// 근거리 x 이내에 적이 있으면 x 이내 모든 적에게 근거리 광역, 없고 R 이내에 적이 있으면 최근접 적에게 원거리 투사체.
        /// R 이내에도 적이 없으면 타이머를 준비 상태로 둔 채 기다린다. 탑승 중·사망 중에는 자동 공격하지 않는다.
        /// </summary>
        private static void TickAutoAttack(World world)
        {
            ref Hero hero = ref world.hero;
            if (hero.attackCooldownTicks > 0)
            {
                hero.attackCooldownTicks--;
            }

            if (hero.state == HeroState.Boarded || hero.state == HeroState.Dead)
            {
                return;
            }
            if (hero.attackCooldownTicks > 0)
            {
                return;
            }

            if (TryMeleeAttack(world) || TryRangedAttack(world))
            {
                world.hero.attackCooldownTicks = world.hero.attackIntervalTicks;
            }
        }

        /// <summary>
        /// 근거리 사거리 안의 모든 적에게 피해를 준다. 피해 도중 적이 죽어 슬롯이 비어도 ID로 조회하므로 안전하다.
        /// </summary>
        private static bool TryMeleeAttack(World world)
        {
            ref Hero hero = ref world.hero;
            List<EntityID> targetEnemyIDs = world.targetEnemyIDs;
            if (TargetingSystem.CollectEnemiesInRadius(world, hero.currentPosition, hero.meleeRange, targetEnemyIDs) == 0)
            {
                return false;
            }

            HeroMeleeAttackRecord meleeRecord = new HeroMeleeAttackRecord();
            meleeRecord.position = hero.currentPosition;
            meleeRecord.radius = hero.meleeRange;
            world.heroMeleeAttackRecords.Add(meleeRecord);

            for (int targetIndex = 0; targetIndex < targetEnemyIDs.Count; targetIndex++)
            {
                EnemySystem.ApplyDamage(world, targetEnemyIDs[targetIndex], hero.attackDamage);
            }
            targetEnemyIDs.Clear();
            return true;
        }

        /// <summary>
        /// 원거리 사거리 안의 최근접 적을 향해 직선 투사체 생성을 요청한다. 피해는 투사체가 명중할 때 ProjectileSystem이 준다.
        /// 투사체는 SpawnSystem이 틱 마지막에 만들므로 다음 틱부터 날아간다. 최대 비행 거리는 원거리 사거리 R이다.
        /// </summary>
        private static bool TryRangedAttack(World world)
        {
            ref Hero hero = ref world.hero;
            EntityID targetID;
            if (!TargetingSystem.TryFindNearestEnemy(world, hero.currentPosition, hero.rangedRange, out targetID))
            {
                return false;
            }

            Vector3 targetPosition = world.enemies.Get(targetID).currentPosition;
            Vector3 direction = targetPosition - hero.currentPosition;
            direction.y = 0f;
            float sqrDistance = direction.sqrMagnitude;
            // 적과 겹쳐 있어 방향이 없으면 히어로가 바라볼 방향을 알 수 없으므로 +Z로 쏜다 (발사 지점에서 바로 명중한다)
            direction = sqrDistance > 0f ? direction / Mathf.Sqrt(sqrDistance) : Vector3.forward;

            ProjectileSpawnRequest request = new ProjectileSpawnRequest();
            request.startPosition = hero.currentPosition;
            request.direction = direction;
            request.speed = hero.projectileSpeed;
            request.radius = hero.projectileRadius;
            request.maxDistance = hero.rangedRange;
            request.damage = hero.attackDamage;
            world.projectileSpawnRequests.Enqueue(request);

            HeroRangedShotRecord shotRecord = new HeroRangedShotRecord();
            shotRecord.startPosition = hero.currentPosition;
            shotRecord.targetID = targetID;
            shotRecord.targetPosition = targetPosition;
            world.heroRangedShotRecords.Add(shotRecord);
            return true;
        }

        /// <summary>
        /// 이번 틱의 이동 거리(moveSpeed)만큼 경로 코너를 따라간다. 한 틱에 코너를 여러 개 지날 수 있다.
        /// 마지막 코너에 닿으면 Idle로 바꾼다.
        /// </summary>
        private static void MoveAlongPath(ref Hero hero)
        {
            float remainingDistance = hero.moveSpeed;
            Vector3 position = hero.currentPosition;

            while (remainingDistance > 0f && hero.nextPathCornerIndex < hero.pathCornerCount)
            {
                ref Vector3 corner = ref hero.pathCorners[hero.nextPathCornerIndex];
                Vector3 toCorner = corner - position;
                float sqrDistanceToCorner = toCorner.sqrMagnitude;

                if (sqrDistanceToCorner <= remainingDistance * remainingDistance)
                {
                    position = corner;
                    remainingDistance -= Mathf.Sqrt(sqrDistanceToCorner);
                    hero.nextPathCornerIndex++;
                }
                else
                {
                    position += toCorner * (remainingDistance / Mathf.Sqrt(sqrDistanceToCorner));
                    remainingDistance = 0f;
                }
            }

            hero.currentPosition = position;

            if (hero.nextPathCornerIndex >= hero.pathCornerCount)
            {
                hero.state = HeroState.Idle;
                hero.pathCornerCount = 0;
                hero.nextPathCornerIndex = 0;
            }
        }
    }
}
