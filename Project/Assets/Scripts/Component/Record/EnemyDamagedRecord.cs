using UnityEngine;

namespace Project05.Component.Record
{
    /// <summary>
    /// 프레임 기록: 적 피격.
    /// </summary>
    public struct EnemyDamagedRecord
    {
        // 적 ID
        public EntityID enemyID;
        // 위치
        public Vector3 position;
        // 피해량
        public int damage;
    }
}
