using UnityEngine;

namespace Project05.Component.Path
{
    /// <summary>
    /// 적 스폰 지점. 씬에서 베이킹된다.
    /// </summary>
    public struct SpawnPoint
    {
        // 스폰 위치
        public Vector3 position;
        // 연결된 경로 인덱스 (World.waypointPaths)
        public int pathIndex;
    }
}
