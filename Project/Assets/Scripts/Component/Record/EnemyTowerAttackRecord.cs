using UnityEngine;

namespace Project05.Component.Record
{
    /// <summary>
    /// 프레임 기록: 적의 타워 공격.
    /// </summary>
    public struct EnemyTowerAttackRecord
    {
        // 적 ID
        public EntityID enemyID;
        // 대상 타워 ID
        public EntityID towerID;
        // 위치
        public Vector3 position;
    }
}
