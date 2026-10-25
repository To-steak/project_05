using System.Collections.Generic;
using Project05.Component;
using Project05.Component.Entity;
using Project05.Component.Record;
using Project05.Component.Request;
using UnityEngine;

namespace Project05.System
{
    /// <summary>
    /// [Tick 4] 히어로 이동과 바라보는 방향, 탑승/하차 상태, 근거리(전방 부채꼴)·원거리(구체) 독립 자동 공격,
    /// 접촉 피해와 무적, 사망·목숨·부활을 처리한다 (DESIGN 3.1).
    /// 구현 브랜치: feat/hero-movement, feat/hero-combat, feat/core-siege, feat/hero-attack-split
    /// </summary>
    public static class HeroSystem
    {
        // 이보다 짧은 이동은 방향 전환에 쓰지 않는다 (제자리에서 방향이 튀는 것을 막음)
        private const float MIN_FACING_SQR_DISTANCE = 0.000001f;

        /// <summary>
        /// 히어로를 한 틱 진행한다.
        /// </summary>
        /// <param name="world">현재 판의 월드</param>
        public static void Tick(World world)
        {
            ref Hero hero = ref world.hero;

            // View 보간 기준점. 움직이지 않는 틱에도 갱신해야 이전 이동이 반복 보간되지 않는다
            hero.previousPosition = hero.currentPosition;
            // 구체는 사망·탑승 중에도 계속 돈다 (다시 보일 때 각도가 튀지 않게)
            UpdateOrbs(world);

            if (hero.state == HeroState.Dead)
            {
                TickRespawn(world);
                return;
            }

            if (hero.state == HeroState.Moving)
            {
                MoveAlongPath(ref hero);
                UpdateFacing(ref hero);
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
            hero.meleeAttackCooldownTicks = 0;
            hero.rangedAttackCooldownTicks = 0;
            hero.invincibilityRemainingTicks = 0;

            HeroRespawnedRecord respawnedRecord = new HeroRespawnedRecord();
            respawnedRecord.position = hero.currentPosition;
            world.heroRespawnedRecords.Add(respawnedRecord);
        }

        /// <summary>
        /// 근거리와 원거리를 각자 타이머로 자동 공격한다 (DESIGN 3.1). 같은 틱에 둘 다 나갈 수 있다.
        /// 조건을 만족하는 적이 없으면 그 공격은 타이머를 준비 상태로 둔 채 기다린다.
        /// 탑승 중·사망 중에는 공격하지 않는다 (타이머는 계속 준다).
        /// </summary>
        private static void TickAutoAttack(World world)
        {
            ref Hero hero = ref world.hero;
            if (hero.meleeAttackCooldownTicks > 0)
            {
                hero.meleeAttackCooldownTicks--;
            }
            if (hero.rangedAttackCooldownTicks > 0)
            {
                hero.rangedAttackCooldownTicks--;
            }

            if (hero.state == HeroState.Boarded || hero.state == HeroState.Dead)
            {
                return;
            }

            if (hero.meleeAttackCooldownTicks <= 0 && TryMeleeAttack(world))
            {
                hero.meleeAttackCooldownTicks = hero.meleeAttackIntervalTicks;
            }
            if (hero.rangedAttackCooldownTicks <= 0 && TryRangedAttack(world))
            {
                hero.rangedAttackCooldownTicks = hero.rangedAttackIntervalTicks;
            }
        }

        /// <summary>
        /// 근거리 사거리 안, 바라보는 방향 기준 부채꼴 안의 모든 적에게 피해를 준다.
        /// 피해 도중 적이 죽어 슬롯이 비어도 ID로 조회하므로 안전하다.
        /// </summary>
        private static bool TryMeleeAttack(World world)
        {
            ref Hero hero = ref world.hero;
            List<EntityID> targetEnemyIDs = world.targetEnemyIDs;
            if (TargetingSystem.CollectEnemiesInSector(world, hero.currentPosition, hero.facingDirection, hero.meleeRange, hero.meleeHalfAngleCosine, targetEnemyIDs) == 0)
            {
                return false;
            }

            HeroMeleeAttackRecord meleeRecord = new HeroMeleeAttackRecord();
            meleeRecord.position = hero.currentPosition;
            meleeRecord.direction = hero.facingDirection;
            meleeRecord.radius = hero.meleeRange;
            meleeRecord.angleDegrees = hero.meleeAngleDegrees;
            world.heroMeleeAttackRecords.Add(meleeRecord);

            for (int targetIndex = 0; targetIndex < targetEnemyIDs.Count; targetIndex++)
            {
                EnemySystem.ApplyDamage(world, targetEnemyIDs[targetIndex], hero.meleeAttackDamage);
            }
            targetEnemyIDs.Clear();
            return true;
        }

        /// <summary>
        /// 원거리 사거리 안의 최근접 적(히어로 기준)을 향해 구체마다 직선 투사체 생성을 요청한다. 피해는 투사체가 명중할 때 ProjectileSystem이 준다.
        /// 투사체는 SpawnSystem이 틱 마지막에 만들므로 다음 틱부터 날아간다. 최대 비행 거리는 원거리 사거리 R이다.
        /// </summary>
        private static bool TryRangedAttack(World world)
        {
            ref Hero hero = ref world.hero;
            EntityID targetID;
            if (hero.orbCount <= 0 || !TargetingSystem.TryFindNearestEnemy(world, hero.currentPosition, hero.rangedRange, out targetID))
            {
                return false;
            }

            Vector3 targetPosition = world.enemies.Get(targetID).currentPosition;
            for (int orbIndex = 0; orbIndex < hero.orbCount; orbIndex++)
            {
                Vector3 orbPosition = GetOrbPosition(in hero, orbIndex);
                Vector3 direction = targetPosition - orbPosition;
                direction.y = 0f;
                float sqrDistance = direction.sqrMagnitude;
                // 구체가 적과 겹쳐 방향이 없으면 바라보는 방향으로 쏜다 (발사 지점에서 바로 명중한다)
                direction = sqrDistance > 0f ? direction / Mathf.Sqrt(sqrDistance) : hero.facingDirection;

                ProjectileSpawnRequest request = new ProjectileSpawnRequest();
                request.startPosition = orbPosition;
                request.direction = direction;
                request.speed = hero.projectileSpeed;
                request.radius = hero.projectileRadius;
                request.maxDistance = hero.rangedRange;
                request.damage = hero.rangedAttackDamage;
                world.projectileSpawnRequests.Enqueue(request);

                HeroRangedShotRecord shotRecord = new HeroRangedShotRecord();
                shotRecord.startPosition = orbPosition;
                shotRecord.targetID = targetID;
                shotRecord.targetPosition = targetPosition;
                world.heroRangedShotRecords.Add(shotRecord);
            }
            return true;
        }

        /// <summary>
        /// 구체 수와 첫 구체 각도를 이번 틱 값으로 갱신한다 (DESIGN 3.1).
        /// 구체 수 = min(시작 수 + 레벨 / 간격, 최대 수). 각도 = tickCount × 틱당 각도 (0 ~ 360).
        /// 각도는 누적하지 않고 틱 수로 매번 계산하므로 오차가 쌓이지 않는다. 이전 각도는 View 보간용이다.
        /// </summary>
        private static void UpdateOrbs(World world)
        {
            ref Hero hero = ref world.hero;
            hero.orbCount = Mathf.Min(hero.orbBaseCount + world.progress.level / hero.orbLevelStep, hero.orbMaxCount);
            hero.previousOrbAngleDegrees = hero.currentOrbAngleDegrees;
            hero.currentOrbAngleDegrees = Mathf.Repeat(world.tickCount * hero.orbDegreesPerTick, 360f);
        }

        /// <summary>
        /// 이번 틱의 구체 위치. 구체 i의 각도 = 첫 구체 각도 + i × 360 / 구체 수, 위치 = 히어로 + (sin, 0, cos) × 반지름.
        /// View(HeroViewSystem)도 같은 식에 보간한 위치·각도를 넣어 그린다.
        /// </summary>
        private static Vector3 GetOrbPosition(in Hero hero, int orbIndex)
        {
            float angleRadians = (hero.currentOrbAngleDegrees + orbIndex * 360f / hero.orbCount) * Mathf.Deg2Rad;
            return hero.currentPosition + new Vector3(Mathf.Sin(angleRadians), 0f, Mathf.Cos(angleRadians)) * hero.orbRadius;
        }

        /// <summary>
        /// 이번 틱에 움직였으면 그 방향을 바라본다 (DESIGN 3.1). 움직이지 않았으면 마지막 방향을 유지한다.
        /// </summary>
        private static void UpdateFacing(ref Hero hero)
        {
            Vector3 movement = hero.currentPosition - hero.previousPosition;
            movement.y = 0f;
            float sqrDistance = movement.sqrMagnitude;
            if (sqrDistance > MIN_FACING_SQR_DISTANCE)
            {
                hero.facingDirection = movement / Mathf.Sqrt(sqrDistance);
            }
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
