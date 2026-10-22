using UnityEngine;

namespace Project05.Component.Record
{
    /// <summary>
    /// 프레임 기록: 적 사망. Logic 슬롯은 이미 비었으므로 연출은 이 값만 쓴다.
    /// </summary>
    public struct EnemyDiedRecord
    {
        // 적 ID
        public EntityID enemyID;
        // 적 종류
        public int enemyTypeIndex;
        // 사망 위치
        public Vector3 position;
    }
}
