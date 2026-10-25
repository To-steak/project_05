using UnityEngine;

namespace Project05.Component.Record
{
    /// <summary>
    /// 프레임 기록: 히어로 원거리 공격. 구체마다 하나씩 남는다.
    /// </summary>
    public struct HeroRangedShotRecord
    {
        // 발사 위치 (구체 위치)
        public Vector3 startPosition;
        // 대상 적 ID
        public EntityID targetID;
        // 대상 위치
        public Vector3 targetPosition;
    }
}
