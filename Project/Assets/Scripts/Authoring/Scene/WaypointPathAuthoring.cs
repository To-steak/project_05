using UnityEngine;

namespace Project05.Authoring.Scene
{
    /// <summary>
    /// 적이 따라가는 한 갈래의 웨이포인트 경로. 자식 Transform의 순서가 웨이포인트 순서이며, 마지막 자식이 코어 쪽이다.
    /// 판 시작 시 SceneBaker가 WaypointPath로 베이킹한다. 실행 중 로직은 없다.
    /// </summary>
    public class WaypointPathAuthoring : MonoBehaviour
    {
        [SerializeField] private Color _gizmoColor = new Color(1f, 0.55f, 0.1f, 1f);

        private const float WAYPOINT_GIZMO_RADIUS = 0.3f;

        /// <summary>
        /// 웨이포인트 수 (자식 Transform 수)
        /// </summary>
        public int WaypointCount
        {
            get { return transform.childCount; }
        }

        /// <summary>
        /// 웨이포인트 하나의 월드 위치를 돌려준다.
        /// </summary>
        /// <param name="waypointIndex">자식 순서 (0이 시작)</param>
        /// <returns>웨이포인트 월드 위치</returns>
        public Vector3 GetWaypointPosition(int waypointIndex)
        {
            return transform.GetChild(waypointIndex).position;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = _gizmoColor;
            int waypointCount = transform.childCount;
            for (int waypointIndex = 0; waypointIndex < waypointCount; waypointIndex++)
            {
                Vector3 waypointPosition = transform.GetChild(waypointIndex).position;
                Gizmos.DrawSphere(waypointPosition, WAYPOINT_GIZMO_RADIUS);
                if (waypointIndex > 0)
                {
                    Gizmos.DrawLine(transform.GetChild(waypointIndex - 1).position, waypointPosition);
                }
            }
        }
    }
}
