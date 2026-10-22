using Project05.Component;

namespace Project05.System
{
    /// <summary>
    /// [Tick 없음] 골드 증감과 해금·건설·업그레이드·수리 비용 지불. Tick 없이 다른 System이 직접 호출한다.
    /// 비용 값 조회(TowerStats)와 거절 기록은 호출하는 쪽이 한다. 이 System은 골드만 다룬다.
    /// </summary>
    public static class EconomySystem
    {
        /// <summary>
        /// 골드를 더한다. 획득 퍼센트는 호출하는 쪽에서 이미 적용한 값을 넘긴다.
        /// </summary>
        /// <param name="world">현재 판의 월드</param>
        /// <param name="amount">더할 골드 (0 이하면 무시)</param>
        public static void AddGold(World world, int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            world.progress.gold += amount;
        }

        /// <summary>
        /// 비용을 낼 골드가 있는지 확인한다.
        /// </summary>
        /// <param name="world">현재 판의 월드</param>
        /// <param name="cost">필요한 골드</param>
        /// <returns>보유 골드가 비용 이상이면 true</returns>
        public static bool CanAfford(World world, int cost)
        {
            return world.progress.gold >= cost;
        }

        /// <summary>
        /// 골드가 충분하면 비용을 지불한다. 부족하면 아무것도 바꾸지 않는다.
        /// 해금·건설·업그레이드·수리에서 쓰며, 실패 시 CommandRejectedRecord(NotEnoughGold)는 호출하는 쪽이 남긴다.
        /// </summary>
        /// <param name="world">현재 판의 월드</param>
        /// <param name="cost">지불할 골드 (0 이상)</param>
        /// <returns>지불했으면 true, 골드가 부족하면 false</returns>
        public static bool TrySpendGold(World world, int cost)
        {
            if (cost < 0)
            {
                return false;
            }

            ref ProgressState progress = ref world.progress;
            if (progress.gold < cost)
            {
                return false;
            }

            progress.gold -= cost;
            return true;
        }
    }
}
