using UnityEngine;

namespace Project05.Component.Record
{
    /// <summary>
    /// 프레임 기록: 타워 피격.
    /// </summary>
    public struct TowerDamagedRecord
    {
        // 타워 ID
        public EntityID towerID;
        // 위치
        public Vector3 position;
        // 피해량
        public int damage;
    }
}
