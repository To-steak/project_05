using Project05.Component;
using Project05.Component.Record;

namespace Project05.System
{
    /// <summary>
    /// [Tick 8] 코어 피해를 처리한다. 코어를 때리는 적(EnemySystem)이 ApplyDamage를 직접 호출한다.
    /// HP가 0이 됐을 때의 패배 판정은 GameFlowSystem이 한 곳에서 한다.
    /// 구현 브랜치: feat/core-siege
    /// </summary>
    public static class CoreSystem
    {
        /// <summary>
        /// 코어 상태를 한 틱 진행한다. 지금은 매 틱 할 일이 없다 (피해는 ApplyDamage로 즉시 처리).
        /// </summary>
        /// <param name="world">현재 판의 월드</param>
        public static void Tick(World world)
        {
        }

        /// <summary>
        /// 코어에 피해를 주고 피격 기록을 남긴다. HP는 0 아래로 내려가지 않는다. 이미 0이면 무시한다.
        /// </summary>
        /// <param name="world">현재 판의 월드</param>
        /// <param name="damage">피해량 (0 이하면 무시)</param>
        public static void ApplyDamage(World world, int damage)
        {
            ref CoreState core = ref world.core;
            if (damage <= 0 || core.currentHP <= 0)
            {
                return;
            }

            core.currentHP = core.currentHP > damage ? core.currentHP - damage : 0;

            CoreDamagedRecord damagedRecord = new CoreDamagedRecord();
            damagedRecord.damage = damage;
            world.coreDamagedRecords.Add(damagedRecord);
        }
    }
}
