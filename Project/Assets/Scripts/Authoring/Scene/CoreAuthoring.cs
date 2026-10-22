using UnityEngine;

namespace Project05.Authoring.Scene
{
    /// <summary>
    /// 코어 위치. 씬에 하나만 둔다. 판 시작 시 SceneBaker가 World.core.position으로 베이킹한다.
    /// 코어 수치(최대 HP)는 GameDefinition에 둔다. 실행 중 로직은 없다.
    /// </summary>
    public class CoreAuthoring : MonoBehaviour
    {
        private const float GIZMO_RADIUS = 1f;

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, GIZMO_RADIUS);
        }
    }
}
