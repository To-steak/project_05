using UnityEngine;

namespace Project05.Component.Entity
{
    /// <summary>
    /// 타워 공터. 씬에서 베이킹되며 World.towerSlots 배열에 둔다.
    /// </summary>
    public struct TowerSlot
    {
        // 공터 위치
        public Vector3 position;
        // 건설된 타워 ID (비어 있으면 무효 ID)
        public EntityID towerID;
        // 건설 요청이 큐에 들어가 처리 대기 중인지 여부
        public bool isBuildPending;
        // 이 공터 타워의 포위 자리 위치 (판 시작 시 World.TOWER_SIEGE_SLOT_COUNT개 할당, 건설 시 타워 종류의 반지름으로 다시 계산)
        public Vector3[] siegeSlotPositions;
        // 포위 자리마다 속한 고리 번호 (0이 가장 안쪽)
        public int[] siegeSlotRingIndices;
        // 포위 자리마다 적이 배정돼 있는지 (건설 시 비움, 배정된 적이 죽으면 비움)
        public bool[] isSiegeSlotOccupied;
    }
}
