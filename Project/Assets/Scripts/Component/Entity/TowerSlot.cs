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
    }
}
