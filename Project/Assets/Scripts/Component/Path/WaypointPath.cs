using UnityEngine;

namespace Project05.Component.Path
{
    /// <summary>
    /// 적이 따라가는 한 갈래의 웨이포인트 경로. 씬에서 베이킹된다.
    /// </summary>
    public struct WaypointPath
    {
        // 웨이포인트 위치 (순서대로, 마지막이 코어 쪽)
        public Vector3[] waypoints;
    }
}
