using UnityEngine;

namespace Project05.Component.Record
{
    /// <summary>
    /// 프레임 기록: 타워 파괴.
    /// </summary>
    public struct TowerDestroyedRecord
    {
        // 타워 ID
        public EntityID towerID;
        // 위치
        public Vector3 position;
    }
}
