using UnityEngine;

namespace Project05.Component
{
    /// <summary>
    /// 코어 상태. 하나뿐이므로 World 필드로 둔다. (Project05.Core 네임스페이스와 충돌하므로 Core라는 이름은 쓰지 않는다)
    /// 포위 자리는 판 시작 시 SessionSystem이 코어 중심 동심원 위에 미리 계산해 둔다 (안쪽 고리부터 번호를 매긴다).
    /// </summary>
    public struct CoreState
    {
        // 코어 위치
        public Vector3 position;
        // 최대 체력
        public int maxHP;
        // 현재 체력
        public int currentHP;
        // 포위 고리 사이 간격 (타워 포위도 같은 값을 쓴다)
        public float siegeRingSpacing;
        // 포위 한 고리 위 자리 사이 간격 (타워 포위도 같은 값을 쓴다)
        public float siegeSlotSpacing;
        // 포위 자리 위치 (인덱스 = 자리 번호, 안쪽 고리부터 순서대로)
        public Vector3[] siegeSlotPositions;
        // 자리마다 속한 고리 번호 (0이 가장 안쪽)
        public int[] siegeSlotRingIndices;
        // 자리마다 적이 배정돼 있는지. 배정된 적이 죽으면 비워 재사용한다
        public bool[] isSiegeSlotOccupied;
    }
}
