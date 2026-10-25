using Project05.Component;
using Project05.Foundation;
using UnityEngine;

namespace Project05.View
{
    /// <summary>
    /// 코어 피격 효과와 화면 경고.
    /// 코어가 맞은 프레임(CoreDamagedRecord)에 코어 위치에 피격 효과를 한 번 내고, 화면 경고를 최대 세기로 올린 뒤 유지 시간 동안 줄인다.
    /// 여러 적이 같은 프레임에 때려도 효과와 경고는 한 번이다 (코어는 하나).
    /// Runner가 LateUpdate에서 정해진 순서로 호출한다. World는 읽기만 한다.
    /// 구현 브랜치: feat/core-siege, feat/vfx
    /// </summary>
    public static class CoreViewSystem
    {
        /// <summary>
        /// 코어 기록을 연출한다.
        /// </summary>
        /// <param name="world">현재 판의 월드 (읽기 전용)</param>
        /// <param name="viewContext">현재 판의 화면 상태</param>
        public static void Present(World world, ViewContext viewContext)
        {
            CoreWarningView warningView = viewContext.coreWarningView;
            if (world.coreDamagedRecords.Count > 0)
            {
                VisualEffectViewSystem.Spawn(viewContext, VisualEffectKind.CoreHit, world.core.position, 1f);
                if (warningView != null)
                {
                    viewContext.coreWarningEndTime = Time.time + warningView.DurationTicks * Time.fixedDeltaTime;
                }
            }

            if (warningView == null)
            {
                return;
            }

            float duration = warningView.DurationTicks * Time.fixedDeltaTime;
            float remaining = viewContext.coreWarningEndTime - Time.time;
            warningView.SetIntensity(remaining <= 0f ? 0f : remaining / duration);
        }
    }
}
