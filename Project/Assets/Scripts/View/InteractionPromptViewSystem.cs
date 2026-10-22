using Project05.Component;
using Project05.Component.Entity;
using UnityEngine;

namespace Project05.View
{
    /// <summary>
    /// 지금 Space를 누르면 반응할 대상(빈 공터 / 타워 / 탑승 중인 타워 / 코어) 위에 Space 아이콘을 하나 띄운다 (DESIGN 2장).
    /// 대상은 CommandSystem이 같은 판정으로 매 틱 World에 기록한 값(interactionTargetKind, interactionTargetPosition)을 읽는다.
    /// 일시정지·패배 중이거나 대상이 없으면 숨긴다.
    /// Runner가 LateUpdate에서 정해진 순서로 호출한다. World는 읽기만 한다.
    /// 구현 브랜치: 4단계 (feat/offscreen-and-prompt)
    /// </summary>
    public static class InteractionPromptViewSystem
    {
        /// <summary>
        /// 상호작용 가능한 대상 위에 Space 아이콘을 표시한다.
        /// </summary>
        /// <param name="world">현재 판의 월드 (읽기 전용)</param>
        /// <param name="viewContext">현재 판의 화면 상태</param>
        public static void Present(World world, ViewContext viewContext)
        {
            InteractionPromptView promptView = viewContext.interactionPromptView;
            if (promptView == null)
            {
                return;
            }

            bool isVisible = world.interactionTargetKind != InteractionTargetKind.None
                && world.pauseReason == PauseReason.None
                && world.flow.phase != GamePhase.Defeat;
            promptView.SetVisible(isVisible);
            if (!isVisible)
            {
                return;
            }

            Transform promptTransform = promptView.transform;
            promptTransform.position = world.interactionTargetPosition + Vector3.up * promptView.HeightOffset;
            Camera mainCamera = viewContext.mainCamera;
            if (promptView.IsFacingCamera && mainCamera != null)
            {
                promptTransform.rotation = mainCamera.transform.rotation;
            }
        }
    }
}
