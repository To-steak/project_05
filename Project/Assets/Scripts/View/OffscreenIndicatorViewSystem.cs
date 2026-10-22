using Project05.Component;
using Project05.Component.Entity;
using UnityEngine;

namespace Project05.View
{
    /// <summary>
    /// 화면 밖에서 코어로 다가오는 적을 화면 가장자리의 방향 화살표로 표시한다 (DESIGN 2장 카메라).
    /// 화면 밖 적 중 코어에 가까운 순으로 최대 화살표 수만큼, 화면 중앙에서 적 방향으로 가장자리(여백 안쪽)에 둔다.
    /// 보간된 위치를 쓰지 않고 현재 틱 위치를 쓴다 (화살표 용도로는 충분하다).
    /// Runner가 LateUpdate에서 정해진 순서로 호출한다. World는 읽기만 한다.
    /// 구현 브랜치: 4단계 (feat/offscreen-and-prompt)
    /// </summary>
    public static class OffscreenIndicatorViewSystem
    {
        // 화살표 스프라이트는 위쪽(+Y)을 가리킨다고 보고 이만큼 돌린다
        private const float ARROW_UP_ANGLE = 90f;

        /// <summary>
        /// 화면 밖 적 방향 화살표를 갱신한다.
        /// </summary>
        /// <param name="world">현재 판의 월드 (읽기 전용)</param>
        /// <param name="viewContext">현재 판의 화면 상태</param>
        public static void Present(World world, ViewContext viewContext)
        {
            RectTransform[] arrows = viewContext.offscreenArrows;
            Camera mainCamera = viewContext.mainCamera;
            if (arrows.Length == 0)
            {
                return;
            }

            int shownCount = 0;
            if (mainCamera != null && world.flow.phase != GamePhase.Defeat)
            {
                shownCount = SelectNearestOffscreenEnemies(world, viewContext, mainCamera, arrows.Length);
            }

            float margin = viewContext.offscreenIndicatorView.EdgeMargin;
            for (int arrowIndex = 0; arrowIndex < arrows.Length; arrowIndex++)
            {
                RectTransform arrow = arrows[arrowIndex];
                bool isVisible = arrowIndex < shownCount;
                if (arrow.gameObject.activeSelf != isVisible)
                {
                    arrow.gameObject.SetActive(isVisible);
                }
                if (isVisible)
                {
                    PlaceArrow(arrow, viewContext.offscreenScreenPoints[arrowIndex], margin);
                }
            }
        }

        /// <summary>
        /// 화면 밖 적 중 코어에 가까운 적을 최대 count개 고르고, 그 화면 좌표를 viewContext.offscreenScreenPoints 앞쪽에 채운다.
        /// 가까운 순으로 끼워 넣는 고정 크기 목록이라 할당이 없다.
        /// </summary>
        /// <returns>고른 적 수</returns>
        private static int SelectNearestOffscreenEnemies(World world, ViewContext viewContext, Camera mainCamera, int count)
        {
            Vector3[] screenPoints = viewContext.offscreenScreenPoints;
            float[] sqrDistances = viewContext.offscreenSqrDistances;
            Vector3 corePosition = world.core.position;
            int selectedCount = 0;

            EntityStore<Enemy> enemies = world.enemies;
            int capacity = enemies.Capacity;
            for (int enemyIndex = 0; enemyIndex < capacity; enemyIndex++)
            {
                if (!enemies.IsAliveAt(enemyIndex))
                {
                    continue;
                }

                Vector3 enemyPosition = enemies.GetAt(enemyIndex).currentPosition;
                Vector3 viewportPoint = mainCamera.WorldToViewportPoint(enemyPosition);
                bool isOnScreen = viewportPoint.z > 0f && viewportPoint.x >= 0f && viewportPoint.x <= 1f && viewportPoint.y >= 0f && viewportPoint.y <= 1f;
                if (isOnScreen)
                {
                    continue;
                }

                float deltaX = enemyPosition.x - corePosition.x;
                float deltaZ = enemyPosition.z - corePosition.z;
                float sqrDistance = deltaX * deltaX + deltaZ * deltaZ;
                if (selectedCount == count && sqrDistance >= sqrDistances[selectedCount - 1])
                {
                    continue;
                }

                // 가까운 순 자리를 찾아 뒤를 한 칸씩 민다
                int insertIndex = selectedCount < count ? selectedCount : count - 1;
                while (insertIndex > 0 && sqrDistances[insertIndex - 1] > sqrDistance)
                {
                    sqrDistances[insertIndex] = sqrDistances[insertIndex - 1];
                    screenPoints[insertIndex] = screenPoints[insertIndex - 1];
                    insertIndex--;
                }
                sqrDistances[insertIndex] = sqrDistance;
                screenPoints[insertIndex] = mainCamera.WorldToScreenPoint(enemyPosition);
                if (selectedCount < count)
                {
                    selectedCount++;
                }
            }
            return selectedCount;
        }

        /// <summary>
        /// 화면 중앙에서 적 화면 좌표 방향으로, 여백 안쪽 사각형의 가장자리에 화살표를 두고 그 방향으로 돌린다.
        /// 적이 카메라 뒤에 있으면(z < 0) 화면 좌표가 뒤집히므로 방향을 반대로 한다. Overlay Canvas라 화면 픽셀 좌표를 그대로 쓴다.
        /// </summary>
        private static void PlaceArrow(RectTransform arrow, Vector3 screenPoint, float margin)
        {
            Vector2 center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            Vector2 direction = new Vector2(screenPoint.x, screenPoint.y) - center;
            if (screenPoint.z < 0f)
            {
                direction = -direction;
            }
            if (direction.sqrMagnitude <= 0f)
            {
                direction = Vector2.up;
            }

            float halfWidth = Mathf.Max(0f, center.x - margin);
            float halfHeight = Mathf.Max(0f, center.y - margin);
            float scaleX = direction.x != 0f ? halfWidth / Mathf.Abs(direction.x) : float.MaxValue;
            float scaleY = direction.y != 0f ? halfHeight / Mathf.Abs(direction.y) : float.MaxValue;
            float scale = Mathf.Min(scaleX, scaleY);

            arrow.position = center + direction * scale;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - ARROW_UP_ANGLE;
            arrow.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }
}
