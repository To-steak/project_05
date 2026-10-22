using UnityEngine;

namespace Project05.Component.Record
{
    /// <summary>
    /// 프레임 기록: 보스 등장.
    /// </summary>
    public struct BossSpawnedRecord
    {
        // 보스 ID
        public EntityID bossID;
        // 등장 위치
        public Vector3 position;
    }
}
