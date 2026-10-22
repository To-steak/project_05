using UnityEngine;

namespace Project05.Authoring.Scene
{
    /// <summary>
    /// 타워 공터 위치. 공터 번호는 하이어라키 순서다 (현재 4개, 코어 중심 같은 반지름·90° 간격).
    /// 판 시작 시 SceneBaker가 TowerSlot으로 베이킹한다. 실행 중 로직은 없다.
    /// </summary>
    public class TowerSlotAuthoring : MonoBehaviour
    {
        private const float GIZMO_WIDTH = 1.5f;
        private const float GIZMO_HEIGHT = 0.1f;

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, new Vector3(GIZMO_WIDTH, GIZMO_HEIGHT, GIZMO_WIDTH));
        }
    }
}
