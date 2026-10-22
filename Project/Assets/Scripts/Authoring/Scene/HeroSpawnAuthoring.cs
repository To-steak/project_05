using UnityEngine;

namespace Project05.Authoring.Scene
{
    /// <summary>
    /// 히어로 시작·부활 위치. 씬에 하나만 둔다. 판 시작 시 SceneBaker가 World.heroSpawnPosition으로 베이킹한다.
    /// 실행 중 로직은 없다.
    /// </summary>
    public class HeroSpawnAuthoring : MonoBehaviour
    {
        private const float GIZMO_RADIUS = 0.5f;

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, GIZMO_RADIUS);
        }
    }
}
