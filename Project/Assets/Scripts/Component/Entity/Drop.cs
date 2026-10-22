using UnityEngine;

namespace Project05.Component.Entity
{
    /// <summary>
    /// 바닥에 떨어진 드롭. 히어로 획득 반경 안에 들어오면 획득된다.
    /// </summary>
    public struct Drop
    {
        // 위치
        public Vector3 position;
        // 종류
        public DropKind kind;
        // 양 (획득 퍼센트 적용 전)
        public int amount;
    }
}
