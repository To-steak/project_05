using UnityEngine;

namespace Project05.System
{
    /// <summary>
    /// [Tick 없음] 포위 자리 계산·배정 (코어와 타워 공용, DESIGN 3.3). 상태 없이 넘겨받은 배열만 다룬다.
    /// - 배치: 중심 동심원 위에 안쪽 고리부터 번호를 매긴다. 고리 r 반지름 = 첫 반지름 + 고리 간격 × r, 자리 수 = max(1, ⌊둘레 ÷ 자리 간격⌋).
    /// - 배정: 빈 자리가 있는 가장 안쪽 고리에서, 오는 적의 위치에 가장 가까운 자리.
    /// 구현 브랜치: feat/tower (feat/core-siege의 코어 포위 계산을 타워와 함께 쓰도록 옮김)
    /// </summary>
    public static class SiegeSlotSystem
    {
        // 배정할 자리가 없을 때의 값
        public const int NO_SIEGE_SLOT = -1;
        // 고리·자리 간격의 하한 (데이터가 0이어도 자리 계산이 끝나도록)
        private const float MIN_SPACING = 0.1f;

        /// <summary>
        /// 중심 둘레에 자리 위치와 고리 번호를 채운다. 배열 길이만큼 자리를 만든다 (할당 없음).
        /// </summary>
        /// <param name="center">포위 중심 (코어·타워 위치)</param>
        /// <param name="firstRingRadius">가장 안쪽 고리 반지름</param>
        /// <param name="ringSpacing">고리 사이 간격</param>
        /// <param name="slotSpacing">한 고리 위 자리 사이 간격</param>
        /// <param name="positions">자리 위치를 채울 배열</param>
        /// <param name="ringIndices">자리마다 고리 번호를 채울 배열 (positions와 같은 길이)</param>
        public static void Layout(Vector3 center, float firstRingRadius, float ringSpacing, float slotSpacing, Vector3[] positions, int[] ringIndices)
        {
            float safeSlotSpacing = Mathf.Max(MIN_SPACING, slotSpacing);
            float safeRingSpacing = Mathf.Max(MIN_SPACING, ringSpacing);
            int slotCount = positions.Length;

            int slotIndex = 0;
            int ringIndex = 0;
            while (slotIndex < slotCount)
            {
                float ringRadius = firstRingRadius + safeRingSpacing * ringIndex;
                int ringSlotCount = Mathf.Max(1, Mathf.FloorToInt(2f * Mathf.PI * ringRadius / safeSlotSpacing));
                for (int ringSlotIndex = 0; ringSlotIndex < ringSlotCount && slotIndex < slotCount; ringSlotIndex++)
                {
                    float angle = 2f * Mathf.PI * ringSlotIndex / ringSlotCount;
                    positions[slotIndex] = center + new Vector3(Mathf.Cos(angle) * ringRadius, 0f, Mathf.Sin(angle) * ringRadius);
                    ringIndices[slotIndex] = ringIndex;
                    slotIndex++;
                }
                ringIndex++;
            }
        }

        /// <summary>
        /// 빈 자리가 있는 가장 안쪽 고리에서 위치에 가장 가까운 자리를 골라 사용 중으로 표시한다.
        /// </summary>
        /// <param name="positions">자리 위치</param>
        /// <param name="ringIndices">자리마다 고리 번호 (안쪽부터 번호가 매겨져 있어야 한다)</param>
        /// <param name="isOccupied">자리마다 사용 여부 (배정한 자리를 true로 바꾼다)</param>
        /// <param name="fromPosition">자리를 받을 적의 현재 위치</param>
        /// <returns>배정한 자리 번호. 빈 자리가 없으면 NO_SIEGE_SLOT</returns>
        public static int TryAssign(Vector3[] positions, int[] ringIndices, bool[] isOccupied, Vector3 fromPosition)
        {
            int bestSlotIndex = NO_SIEGE_SLOT;
            int bestRingIndex = int.MaxValue;
            float bestSqrDistance = float.MaxValue;

            for (int slotIndex = 0; slotIndex < isOccupied.Length; slotIndex++)
            {
                if (isOccupied[slotIndex])
                {
                    continue;
                }

                int ringIndex = ringIndices[slotIndex];
                if (ringIndex > bestRingIndex)
                {
                    // 더 바깥 고리는 볼 필요가 없다
                    break;
                }

                float sqrDistance = TargetingSystem.GetSqrDistanceXZ(fromPosition, positions[slotIndex]);
                if (ringIndex < bestRingIndex || sqrDistance < bestSqrDistance)
                {
                    bestRingIndex = ringIndex;
                    bestSqrDistance = sqrDistance;
                    bestSlotIndex = slotIndex;
                }
            }

            if (bestSlotIndex != NO_SIEGE_SLOT)
            {
                isOccupied[bestSlotIndex] = true;
            }
            return bestSlotIndex;
        }
    }
}
