using Project05.Component;
using UnityEngine;

namespace Project05.View
{
    /// <summary>
    /// ViewSystem 공용 보간 계산 (DESIGN 5.11). 상태 없는 순수 함수만 둔다.
    /// </summary>
    public static class ViewInterpolation
    {
        /// <summary>
        /// 시뮬레이션이 멈췄는지 (일시정지 또는 패배). 멈춘 동안 View도 시간 흐름이 있는 연출(효과, 문구, 경고)을 멈춘다.
        /// </summary>
        /// <param name="world">현재 판의 월드 (읽기 전용)</param>
        /// <returns>틱이 진행되지 않는 상태면 true</returns>
        public static bool IsSimulationStopped(World world)
        {
            return world.pauseReason != PauseReason.None || world.flow.phase == GamePhase.Defeat;
        }

        /// <summary>
        /// 직전 틱과 이번 틱 사이 보간 비율. 시뮬레이션이 멈춘 동안(일시정지, 패배)에는 틱이 진행되지 않으므로 현재 위치(1)에 고정한다.
        /// </summary>
        /// <param name="world">현재 판의 월드 (읽기 전용)</param>
        /// <returns>0 ~ 1 보간 비율</returns>
        public static float GetAlpha(World world)
        {
            if (IsSimulationStopped(world))
            {
                return 1f;
            }

            return Mathf.Clamp01((Time.time - Time.fixedTime) / Time.fixedDeltaTime);
        }
    }
}
