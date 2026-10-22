using UnityEngine;

namespace Project05.Authoring.Scene
{
    /// <summary>
    /// 적 스폰 지점. 자기 위치와 연결된 경로를 판 시작 시 SceneBaker가 SpawnPoint로 베이킹한다. 실행 중 로직은 없다.
    /// </summary>
    public class SpawnPointAuthoring : MonoBehaviour
    {
        [SerializeField] private WaypointPathAuthoring _path;

        private const float GIZMO_RADIUS = 0.6f;

        /// <summary>
        /// 이 스폰 지점에서 나온 적이 따라갈 경로 (없으면 null)
        /// </summary>
        public WaypointPathAuthoring Path
        {
            get { return _path; }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, GIZMO_RADIUS);
            if (_path != null && _path.WaypointCount > 0)
            {
                Gizmos.DrawLine(transform.position, _path.GetWaypointPosition(0));
            }
        }
    }
}
