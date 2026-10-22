using UnityEngine;

namespace Project05.Component.Record
{
    /// <summary>
    /// 프레임 기록: 히어로 피격.
    /// </summary>
    public struct HeroDamagedRecord
    {
        // 피격 위치
        public Vector3 position;
        // 피해량
        public int damage;
    }
}
