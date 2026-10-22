using Project05.Component;
using Project05.Component.Entity;
using UnityEngine;

namespace Project05.View
{
    /// <summary>
    /// 히어로 보간, 이동·공격 애니메이션, 무적 깜빡임, 사망·부활 연출.
    /// Runner가 LateUpdate에서 정해진 순서로 호출한다. World는 읽기만 한다.
    /// 구현 브랜치: feat/hero-movement, feat/hero-combat, feat/core-siege (무적 깜빡임, 사망 시 숨김)
    /// </summary>
    public static class HeroViewSystem
    {
        // 이보다 짧은 이동은 방향 전환에 쓰지 않는다 (제자리에서 방향이 튀는 것을 막음)
        private const float MIN_FACING_SQR_DISTANCE = 0.000001f;

        /// <summary>
        /// 히어로 View를 World에 맞춰 갱신하고 히어로 기록을 연출한다.
        /// </summary>
        /// <param name="world">현재 판의 월드 (읽기 전용)</param>
        /// <param name="viewContext">현재 판의 화면 상태</param>
        public static void Present(World world, ViewContext viewContext)
        {
            HeroView heroView = viewContext.heroView;
            if (heroView == null)
            {
                return;
            }

            ref readonly Hero hero = ref world.hero;
            float alpha = ViewInterpolation.GetAlpha(world);
            Vector3 position = Vector3.LerpUnclamped(hero.previousPosition, hero.currentPosition, alpha);

            // 이번 틱의 이동 방향을 바라본다. 멈춰 있으면 마지막 방향을 유지한다
            Vector3 movement = hero.currentPosition - hero.previousPosition;
            movement.y = 0f;

            Transform heroTransform = heroView.transform;
            if (movement.sqrMagnitude > MIN_FACING_SQR_DISTANCE)
            {
                heroTransform.SetPositionAndRotation(position, Quaternion.LookRotation(movement));
            }
            else
            {
                heroTransform.position = position;
            }

            heroView.SetVisible(IsHeroVisible(in hero, heroView.InvincibilityBlinkTicks));
        }

        /// <summary>
        /// 부활 대기 중에는 숨기고, 피격 무적 중에는 남은 무적 틱으로 깜빡인다 (DESIGN 3.1: 깜빡임은 View가 표현).
        /// 남은 틱으로 계산하므로 일시정지 중에는 깜빡임도 멈춘다.
        /// </summary>
        private static bool IsHeroVisible(in Hero hero, int blinkTicks)
        {
            if (hero.state == HeroState.Dead)
            {
                return false;
            }
            if (hero.invincibilityRemainingTicks <= 0)
            {
                return true;
            }
            return (hero.invincibilityRemainingTicks / Mathf.Max(1, blinkTicks)) % 2 == 0;
        }
    }
}
