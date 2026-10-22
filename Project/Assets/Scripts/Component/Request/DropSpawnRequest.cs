using Project05.Component.Entity;
using UnityEngine;

namespace Project05.Component.Request
{
    /// <summary>
    /// 드롭 생성 요청.
    /// </summary>
    public struct DropSpawnRequest
    {
        // 종류
        public DropKind kind;
        // 양
        public int amount;
        // 떨어질 위치
        public Vector3 position;
    }
}
