using UnityEngine;

namespace Project05.Component.Record
{
    /// <summary>
    /// 프레임 기록: 히어로 근거리 광역 공격.
    /// </summary>
    public struct HeroMeleeAttackRecord
    {
        // 공격 위치 (히어로 위치)
        public Vector3 position;
        // 공격 반경
        public float radius;
    }
}
