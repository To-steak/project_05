using Project05.Component.Entity;
using UnityEngine;

namespace Project05.Component.Record
{
    /// <summary>
    /// 프레임 기록: 드롭 획득.
    /// </summary>
    public struct DropCollectedRecord
    {
        // 종류
        public DropKind kind;
        // 획득량 (퍼센트 적용 후)
        public int amount;
        // 위치
        public Vector3 position;
    }
}
