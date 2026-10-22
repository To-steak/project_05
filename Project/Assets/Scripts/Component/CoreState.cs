using UnityEngine;

namespace Project05.Component
{
    /// <summary>
    /// 코어 상태. 하나뿐이므로 World 필드로 둔다. (Project05.Core 네임스페이스와 충돌하므로 Core라는 이름은 쓰지 않는다)
    /// </summary>
    public struct CoreState
    {
        // 코어 위치
        public Vector3 position;
        // 최대 체력
        public int maxHP;
        // 현재 체력
        public int currentHP;
        // 포위 자리 사용 현황: 지금까지 배정한 자리 수 (다음 배정 자리 번호)
        public int siegeSlotAssignedCount;
    }
}
