using Project05.Component;
using Project05.Component.Data;
using Project05.Component.Entity;
using Project05.Component.Input;
using Project05.Component.Record;
using Project05.Component.Request;
using UnityEngine;

namespace Project05.System
{
    /// <summary>
    /// [Tick 5] 미탑승 타워의 자동 공격과 탑승 시 클릭 사격(공격력 = 히어로 공격력)을 처리한다. 업그레이드·수리 처리 함수도 여기에 둔다.
    /// feat/tower ①: 미탑승 자동 공격. 공격 타이머가 준비되면 사거리 안 최근접 적에게 직선 투사체를 쏜다 (히어로 원거리와 같은 ProjectileSystem).
    /// feat/tower ②: 탑승 중에는 자동 공격 대신, 좌클릭을 누르고 있는 동안 공격 타이머가 준비될 때마다 커서 방향으로 쏜다 (공격력 = 히어로 공격력).
    /// 타워 피해와 파괴(공터 비움, 탑승 중이면 강제 하차, 공격하던 적은 경로로 복귀).
    /// 구현 브랜치: feat/tower (자동 공격 → 탑승 사격·HP·파괴), feat/core-menu (업그레이드·수리)
    /// </summary>
    public static class TowerSystem
    {
        /// <summary>
        /// 모든 타워를 한 틱 진행한다.
        /// </summary>
        /// <param name="world">현재 판의 월드</param>
        public static void Tick(World world)
        {
            EntityStore<Tower> towers = world.towers;
            int capacity = towers.Capacity;
            for (int towerIndex = 0; towerIndex < capacity; towerIndex++)
            {
                if (!towers.IsAliveAt(towerIndex))
                {
                    continue;
                }

                ref Tower tower = ref towers.GetAt(towerIndex);
                if (tower.attackCooldownTicks > 0)
                {
                    tower.attackCooldownTicks--;
                }

                if (tower.attackCooldownTicks > 0)
                {
                    continue;
                }

                // 탑승 중에는 자동 공격이 꺼지고, 누르고 있는 동안 커서 방향으로 사격한다
                bool hasFired = tower.isBoarded
                    ? TryBoardedFire(world, towers.GetIDAt(towerIndex), ref tower)
                    : TryAutoAttack(world, towers.GetIDAt(towerIndex), ref tower);
                if (hasFired)
                {
                    tower.attackCooldownTicks = tower.attackIntervalTicks;
                }
            }
        }

        /// <summary>
        /// 타워 종류의 해당 단계 수치(최대 HP, 공격력, 사거리, 공격 주기)를 타워에 복사한다. 건설·업그레이드 시점에만 테이블을 본다.
        /// 현재 HP는 바꾸지 않는다 (건설·업그레이드에서 호출하는 쪽이 정한다).
        /// </summary>
        /// <param name="world">현재 판의 월드</param>
        /// <param name="tower">대상 타워</param>
        /// <param name="level">적용할 단계 (0부터, 테이블 범위로 자른다)</param>
        public static void ApplyLevel(World world, ref Tower tower, int level)
        {
            TowerLevelStats[] levels = world.towerStats[tower.towerTypeIndex].levels;
            tower.level = Mathf.Clamp(level, 0, levels.Length - 1);
            ref TowerLevelStats levelStats = ref levels[tower.level];
            tower.maxHP = levelStats.maxHP;
            tower.attackDamage = levelStats.attackDamage;
            tower.range = levelStats.range;
            tower.attackIntervalTicks = levelStats.attackIntervalTicks;
        }

        /// <summary>
        /// 타워에 피해를 주고 피격 기록을 남긴다. HP가 0이 되면 그 자리에서 파괴한다. 이미 없는 타워면 무시한다.
        /// </summary>
        /// <param name="world">현재 판의 월드</param>
        /// <param name="towerID">피해를 받을 타워</param>
        /// <param name="damage">피해량 (0 이하면 무시)</param>
        public static void ApplyDamage(World world, EntityID towerID, int damage)
        {
            if (damage <= 0 || !world.towers.IsAlive(towerID))
            {
                return;
            }

            ref Tower tower = ref world.towers.Get(towerID);
            tower.currentHP = tower.currentHP > damage ? tower.currentHP - damage : 0;

            TowerDamagedRecord damagedRecord = new TowerDamagedRecord();
            damagedRecord.towerID = towerID;
            damagedRecord.position = tower.position;
            damagedRecord.damage = damage;
            world.towerDamagedRecords.Add(damagedRecord);

            if (tower.currentHP <= 0)
            {
                DestroyTower(world, towerID, ref tower);
            }
        }

        /// <summary>
        /// 타워를 파괴한다 (DESIGN 3.2). 공터를 비우고(다시 건설해야 하며 업그레이드도 초기화), 탑승 중이면 히어로를 강제 하차시키고,
        /// 이 타워를 공격하던 적은 경로 추종으로 되돌린 뒤 슬롯을 즉시 비운다. 종류 해금은 유지된다.
        /// </summary>
        private static void DestroyTower(World world, EntityID towerID, ref Tower tower)
        {
            TowerDestroyedRecord destroyedRecord = new TowerDestroyedRecord();
            destroyedRecord.towerID = towerID;
            destroyedRecord.position = tower.position;
            world.towerDestroyedRecords.Add(destroyedRecord);

            world.towerSlots[tower.slotIndex].towerID = default(EntityID);

            if (tower.isBoarded)
            {
                HeroSystem.Dismount(world);
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
                if (enemy.targetTowerID == towerID)
                {
                    EnemySystem.ReturnToPath(ref enemy);
                }
            }

            world.towers.Destroy(towerID);
        }

        /// <summary>
        /// 탑승 사격: 좌클릭을 누르고 있고 커서가 지면을 가리키면 그 방향으로 직선 투사체를 쏜다. 공격력은 히어로 공격력, 최대 비행 거리는 타워 사거리.
        /// 누르고 있지 않으면 타이머를 준비 상태로 둔 채 기다린다 (누르는 순간 바로 쏜다).
        /// </summary>
        private static bool TryBoardedFire(World world, EntityID towerID, ref Tower tower)
        {
            InputBuffer input = world.input;
            if (!input.isPointerHeld || !input.hasHeldGroundPosition)
            {
                return false;
            }

            Vector3 targetPosition = input.heldGroundPosition;
            Vector3 direction = targetPosition - tower.position;
            direction.y = 0f;
            float sqrDistance = direction.sqrMagnitude;
            if (sqrDistance <= 0f)
            {
                return false;
            }
            direction /= Mathf.Sqrt(sqrDistance);

            RequestProjectile(world, in tower, direction, world.hero.attackDamage);

            TowerShotRecord shotRecord = new TowerShotRecord();
            shotRecord.towerID = towerID;
            shotRecord.targetPosition = targetPosition;
            shotRecord.isBoardedShot = true;
            world.towerShotRecords.Add(shotRecord);
            return true;
        }

        private static void RequestProjectile(World world, in Tower tower, Vector3 direction, int damage)
        {
            ProjectileSpawnRequest request = new ProjectileSpawnRequest();
            request.startPosition = tower.position;
            request.direction = direction;
            request.speed = tower.projectileSpeed;
            request.radius = tower.projectileRadius;
            request.maxDistance = tower.range;
            request.damage = damage;
            world.projectileSpawnRequests.Enqueue(request);
        }

        /// <summary>
        /// 사거리 안 최근접 적 방향으로 직선 투사체 생성을 요청한다. 최대 비행 거리는 사거리다.
        /// </summary>
        private static bool TryAutoAttack(World world, EntityID towerID, ref Tower tower)
        {
            EntityID targetID;
            if (!TargetingSystem.TryFindNearestEnemy(world, tower.position, tower.range, out targetID))
            {
                return false;
            }

            Vector3 targetPosition = world.enemies.Get(targetID).currentPosition;
            Vector3 direction = targetPosition - tower.position;
            direction.y = 0f;
            float sqrDistance = direction.sqrMagnitude;
            direction = sqrDistance > 0f ? direction / Mathf.Sqrt(sqrDistance) : Vector3.forward;

            RequestProjectile(world, in tower, direction, tower.attackDamage);

            TowerShotRecord shotRecord = new TowerShotRecord();
            shotRecord.towerID = towerID;
            shotRecord.targetPosition = targetPosition;
            shotRecord.isBoardedShot = false;
            world.towerShotRecords.Add(shotRecord);
            return true;
        }
    }
}
