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

            if (hero.state == HeroState.Dead)
            {
                TickRespawn(world);
                return;
            }

            if (hero.state == HeroState.Moving)
            {
                MoveAlongPath(ref hero);
            }

            if (hero.invincibilityRemainingTicks > 0)
            {
                hero.invincibilityRemainingTicks--;
            }

            TakeContactDamage(world);
            if (world.hero.state == HeroState.Dead)
            {
                return;
            }

            TickAutoAttack(world);
        }

        /// <summary>
        /// 지상의 히어로를 타워에 태운다 (DESIGN 3.2). 히어로는 타워 위치에 고정되고, 자동 공격·접촉 피해·드롭 획득이 꺼진다.
        /// 이동 경로는 버리고, 타워 위치로 순간 이동하므로 이전 위치도 같이 옮긴다.
        /// </summary>
        /// <param name="world">현재 판의 월드</param>
        /// <param name="towerID">탈 타워 (살아있어야 한다)</param>
        /// <returns>탑승했으면 true</returns>
        public static bool Board(World world, EntityID towerID)
        {
            ref Hero hero = ref world.hero;
            if ((hero.state != HeroState.Idle && hero.state != HeroState.Moving) || !world.towers.IsAlive(towerID))
            {
                return false;
            }

            ref Tower tower = ref world.towers.Get(towerID);
            tower.isBoarded = true;
            hero.state = HeroState.Boarded;
            hero.boardedTowerID = towerID;
            hero.pathCornerCount = 0;
            hero.nextPathCornerIndex = 0;
            hero.previousPosition = tower.position;
            hero.currentPosition = tower.position;
            hero.moveTargetPosition = tower.position;
            return true;
        }

        /// <summary>
        /// 탑승 중인 히어로를 내린다. 히어로는 타워 자리에 선 채로 지상(Idle) 상태가 된다.
        /// Space로 하차할 때와 타워가 파괴돼 강제 하차할 때 쓴다. 탑승 중이 아니면 아무것도 하지 않는다.
        /// </summary>
        /// <param name="world">현재 판의 월드</param>
        public static void Dismount(World world)
        {
            ref Hero hero = ref world.hero;
            if (hero.state != HeroState.Boarded)
            {
                return;
            }

            if (world.towers.IsAlive(hero.boardedTowerID))
            {
                world.towers.Get(hero.boardedTowerID).isBoarded = false;
            }
            hero.state = HeroState.Idle;
            hero.boardedTowerID = default(EntityID);
        }

        /// <summary>
        /// 적과 닿았으면 접촉 피해를 받는다 (DESIGN 3.1). 닿음 = XZ 거리 ≤ 히어로 접촉 반경 + 적 반경. 적은 멈추지 않는다.
        /// 피해를 받으면 invincibilityTicks 동안 추가 피해를 받지 않는다. 탑승 중에는 피해를 받지 않는다.
        /// 한 틱에 여러 적과 닿아도 무적이 바로 걸리므로 첫 적의 피해만 받는다.
        /// </summary>
        private static void TakeContactDamage(World world)
        {
            ref Hero hero = ref world.hero;
            if (hero.state == HeroState.Boarded || hero.invincibilityRemainingTicks > 0)
            {
                return;
            }

            EntityStore<Enemy> enemies = world.enemies;
            int capacity = enemies.Capacity;
            for (int enemyIndex = 0; enemyIndex < capacity; enemyIndex++)
            {
                if (!enemies.IsAliveAt(enemyIndex))
                {
                    continue;
                }

                ref Enemy enemy = ref enemies.GetAt(enemyIndex);
                float contactDistance = hero.contactRadius + enemy.radius;
                if (TargetingSystem.GetSqrDistanceXZ(hero.currentPosition, enemy.currentPosition) > contactDistance * contactDistance)
                {
                    continue;
                }
                if (enemy.contactDamage <= 0)
                {
                    continue;
                }

                ApplyDamage(world, enemy.contactDamage);
                return;
            }
        }

        /// <summary>
        /// 히어로에 피해를 주고 무적을 건다. HP가 0이 되면 목숨 하나를 잃고 부활 대기(Dead)로 바뀐다.
        /// 목숨이 0이 되면 부활하지 않으며, 패배 판정은 GameFlowSystem이 한다.
        /// </summary>
        private static void ApplyDamage(World world, int damage)
        {
            ref Hero hero = ref world.hero;
            hero.currentHP = hero.currentHP > damage ? hero.currentHP - damage : 0;
            hero.invincibilityRemainingTicks = hero.invincibilityTicks;

            HeroDamagedRecord damagedRecord = new HeroDamagedRecord();
            damagedRecord.position = hero.currentPosition;
            damagedRecord.damage = damage;
            world.heroDamagedRecords.Add(damagedRecord);

            if (hero.currentHP > 0)
            {
                return;
            }

            hero.remainingLives = hero.remainingLives > 0 ? hero.remainingLives - 1 : 0;
            hero.state = HeroState.Dead;
            hero.respawnRemainingTicks = hero.respawnTicks;
            hero.pathCornerCount = 0;
            hero.nextPathCornerIndex = 0;
            hero.invincibilityRemainingTicks = 0;

            HeroDiedRecord diedRecord = new HeroDiedRecord();
            diedRecord.position = hero.currentPosition;
            diedRecord.remainingLives = hero.remainingLives;
            world.heroDiedRecords.Add(diedRecord);
        }

        /// <summary>
        /// 부활 대기 시간이 지나면 부활 지점에서 HP 가득 찬 상태로 부활한다. 목숨이 남아 있지 않으면 계속 대기한다.
        /// 부활 지점으로 순간 이동하므로 이전 위치도 같이 옮겨 View가 보간하며 미끄러지지 않게 한다.
        /// </summary>
        private static void TickRespawn(World world)
        {
            ref Hero hero = ref world.hero;
            if (hero.remainingLives <= 0)
            {
                return;
            }

            if (hero.respawnRemainingTicks > 0)
            {
                hero.respawnRemainingTicks--;
                if (hero.respawnRemainingTicks > 0)
                {
                    return;
                }
            }

            hero.state = HeroState.Idle;
            hero.currentHP = hero.maxHP;
            hero.previousPosition = world.heroSpawnPosition;
            hero.currentPosition = world.heroSpawnPosition;
            hero.attackCooldownTicks = 0;
            hero.invincibilityRemainingTicks = 0;

            HeroRespawnedRecord respawnedRecord = new HeroRespawnedRecord();
            respawnedRecord.position = hero.currentPosition;
            world.heroRespawnedRecords.Add(respawnedRecord);
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
