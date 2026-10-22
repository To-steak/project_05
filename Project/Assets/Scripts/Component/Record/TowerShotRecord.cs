using UnityEngine;

namespace Project05.Component.Record
{
    /// <summary>
    /// 프레임 기록: 타워 사격.
    /// </summary>
    public struct TowerShotRecord
    {
        // 타워 ID
        public EntityID towerID;
        // 대상 위치
        public Vector3 targetPosition;
        // 탑승 사격 여부
        public bool isBoardedShot;
    }
}
