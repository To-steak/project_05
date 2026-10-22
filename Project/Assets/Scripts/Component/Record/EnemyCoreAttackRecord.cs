using UnityEngine;

namespace Project05.Component.Record
{
    /// <summary>
    /// 프레임 기록: 적의 코어 공격.
    /// </summary>
    public struct EnemyCoreAttackRecord
    {
        // 적 ID
        public EntityID enemyID;
        // 위치
        public Vector3 position;
    }
}
