using UnityEngine;

namespace Project05.Component.Record
{
    /// <summary>
    /// 프레임 기록: 타워 건설.
    /// </summary>
    public struct TowerBuiltRecord
    {
        // 타워 ID
        public EntityID towerID;
        // 위치
        public Vector3 position;
    }
}
