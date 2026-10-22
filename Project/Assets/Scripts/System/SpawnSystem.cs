using System.Collections.Generic;
using Project05.Component;
using Project05.Component.Entity;
using Project05.Component.Request;

namespace Project05.System
{
    /// <summary>
    /// [Tick 9] 생성 요청 큐를 처리해 적·보스·드롭·타워를 실제로 만든다. 테이블 조회, 난이도 수식 계산, 엔티티 값 복사를 여기서 한다. 용량이 차서 실패한 요청은 다음 틱에 다시 시도한다.
    /// 구현 브랜치: feat/enemy-wave(적·보스), feat/drop-progress(드롭), feat/tower(타워)
    /// </summary>
    public static class SpawnSystem
    {
        /// <summary>
        /// 생성 요청 큐를 처리한다. 틱의 마지막에 실행된다.
        /// </summary>
        /// <param name="world">현재 판의 월드</param>
        public static void Tick(World world)
        {
            SpawnDrops(world);
        }

        /// <summary>
        /// 드롭 생성 요청을 앞에서부터 처리한다. 저장소가 가득 차면 남은 요청은 큐에 둔 채 다음 틱에 다시 시도한다.
        /// </summary>
        private static void SpawnDrops(World world)
        {
            Queue<DropSpawnRequest> requests = world.dropSpawnRequests;
            EntityStore<Drop> drops = world.drops;

            while (requests.Count > 0)
            {
                EntityID dropID;
                if (!drops.TryCreate(out dropID))
                {
                    return;
                }

                DropSpawnRequest request = requests.Dequeue();
                ref Drop drop = ref drops.Get(dropID);
                drop.position = request.position;
                drop.kind = request.kind;
                drop.amount = request.amount;
            }
        }
    }
}
