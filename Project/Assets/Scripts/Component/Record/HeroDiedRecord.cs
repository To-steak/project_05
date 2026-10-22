using UnityEngine;

namespace Project05.Component.Record
{
    /// <summary>
    /// 프레임 기록: 히어로 사망.
    /// </summary>
    public struct HeroDiedRecord
    {
        // 사망 위치
        public Vector3 position;
        // 남은 목숨
        public int remainingLives;
    }
}
