using UnityEngine;

namespace Project05.Component.Record
{
    /// <summary>
    /// 프레임 기록: 폭발 투사체(탑승 사격)가 명중·소진 지점에서 터졌다.
    /// </summary>
    public struct ProjectileExplodedRecord
    {
        // 폭발 중심
        public Vector3 position;
        // 폭발 반경
        public float radius;
    }
}
