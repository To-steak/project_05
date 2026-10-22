using Project05.Component;
using Project05.Component.Entity;
using Project05.Component.Record;
using Project05.Component.Request;
using UnityEngine;

namespace Project05.System
{
    /// <summary>
    /// [Tick 7] 히어로 획득 반경 안의 드롭을 획득한다 (탑승 중·사망 중 불가). 획득 퍼센트를 적용한다.
    /// 드롭 생성 요청(RequestSpawn)도 여기서 제공한다. 적 사망 처리(EnemySystem)가 호출한다.
    /// </summary>
    public static class DropSystem
    {
        private const int PERCENT_BASE = 100;
        private const int PERCENT_ROUNDING_OFFSET = PERCENT_BASE / 2;

        /// <summary>
        /// 획득 반경 안의 드롭을 획득 처리한다. 획득량에 퍼센트를 적용해 골드·XP에 더하고 기록을 남긴 뒤 드롭을 즉시 소멸시킨다.
        /// XP로 레벨업하면 ProgressSystem이 일시정지를 걸며, 이번 틱의 나머지 획득은 그대로 처리한다.
        /// </summary>
        /// <param name="world">현재 판의 월드</param>
        public static void Tick(World world)
        {
            ref Hero hero = ref world.hero;
            if (hero.state == HeroState.Boarded || hero.state == HeroState.Dead)
            {
                return;
            }

            Vector3 heroPosition = hero.currentPosition;
            float pickupRadiusSquared = hero.pickupRadius * hero.pickupRadius;
            EntityStore<Drop> drops = world.drops;

            for (int dropIndex = 0; dropIndex < drops.Capacity; dropIndex++)
            {
                if (!drops.IsAliveAt(dropIndex))
                {
                    continue;
                }

                ref Drop drop = ref drops.GetAt(dropIndex);
                float deltaX = drop.position.x - heroPosition.x;
                float deltaZ = drop.position.z - heroPosition.z;
                if (deltaX * deltaX + deltaZ * deltaZ > pickupRadiusSquared)
                {
                    continue;
                }

                Collect(world, in drop);
                drops.Destroy(drops.GetIDAt(dropIndex));
            }
        }

        /// <summary>
        /// 적이 죽은 자리에 골드·XP 드롭 생성 요청을 넣는다. 실제 생성은 SpawnSystem이 틱 마지막에 한다.
        /// 양이 0 이하인 종류는 요청하지 않는다.
        /// </summary>
        /// <param name="world">현재 판의 월드</param>
        /// <param name="position">떨어질 위치 (적 사망 위치)</param>
        /// <param name="gold">드롭 골드 (난이도 적용 후, 획득 퍼센트 적용 전)</param>
        /// <param name="xp">드롭 XP (난이도 적용 후, 획득 퍼센트 적용 전)</param>
        public static void RequestSpawn(World world, Vector3 position, int gold, int xp)
        {
            if (gold > 0)
            {
                DropSpawnRequest goldRequest = new DropSpawnRequest();
                goldRequest.kind = DropKind.Gold;
                goldRequest.amount = gold;
                goldRequest.position = position;
                world.dropSpawnRequests.Enqueue(goldRequest);
            }

            if (xp > 0)
            {
                DropSpawnRequest xpRequest = new DropSpawnRequest();
                xpRequest.kind = DropKind.XP;
                xpRequest.amount = xp;
                xpRequest.position = position;
                world.dropSpawnRequests.Enqueue(xpRequest);
            }
        }

        /// <summary>
        /// 양에 획득 퍼센트를 적용한다. 정수 반올림 (DESIGN 3.4.1의 round와 같은 방식).
        /// </summary>
        /// <param name="amount">원래 양</param>
        /// <param name="gainPercent">획득 퍼센트 (100 = 1배)</param>
        /// <returns>퍼센트 적용 후 양</returns>
        public static int ApplyGainPercent(int amount, int gainPercent)
        {
            return (amount * gainPercent + PERCENT_ROUNDING_OFFSET) / PERCENT_BASE;
        }

        private static void Collect(World world, in Drop drop)
        {
            ref ProgressState progress = ref world.progress;

            int collectedAmount;
            if (drop.kind == DropKind.Gold)
            {
                collectedAmount = ApplyGainPercent(drop.amount, progress.goldGainPercent);
                EconomySystem.AddGold(world, collectedAmount);
            }
            else
            {
                collectedAmount = ApplyGainPercent(drop.amount, progress.xpGainPercent);
                ProgressSystem.AddXP(world, collectedAmount);
            }

            DropCollectedRecord record = new DropCollectedRecord();
            record.kind = drop.kind;
            record.amount = collectedAmount;
            record.position = drop.position;
            world.dropCollectedRecords.Add(record);
        }
    }
}
