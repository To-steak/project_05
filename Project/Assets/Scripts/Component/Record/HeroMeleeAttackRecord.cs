using UnityEngine;

namespace Project05.Component.Record
{
    /// <summary>
    /// 프레임 기록: 히어로 근거리 공격 (전방 부채꼴).
    /// </summary>
    public struct HeroMeleeAttackRecord
    {
        // 공격 위치 (히어로 위치)
        public Vector3 position;
        // 공격 방향 (히어로가 바라보는 방향, XZ 단위 벡터)
        public Vector3 direction;
        // 공격 반경
        public float radius;
        // 부채꼴 각도 (도)
        public float angleDegrees;
    }
}
