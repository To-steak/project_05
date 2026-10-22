using UnityEngine;

namespace Project05.Authoring.Scene
{
    /// <summary>
    /// 히어로 시작·부활 위치. Game 씬에 하나 배치하며, 판 시작 시 SceneBaker가 World.heroSpawnPosition으로 베이킹한다.
    /// NavMesh 위(또는 가까이)에 둔다.
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
