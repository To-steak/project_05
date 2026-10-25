using System.Collections.Generic;
using Project05.Component;
using Project05.Component.Entity;
using Project05.Component.Record;
using Project05.Foundation;
using UnityEngine;

namespace Project05.View
{
    /// <summary>
    /// 히어로 보간, 이동·공격 애니메이션, 무적 깜빡임, 사망·부활 연출.
    /// Runner가 LateUpdate에서 정해진 순서로 호출한다. World는 읽기만 한다.
    /// feat/vfx: 피격·근거리 범위·사망·부활 효과 (히어로 기록).
    /// feat/animation: 이동 블렌드(Logic 이동 거리 ÷ 이동 속도), 근거리 공격(동작 차례로)·피격·사망·부활(히어로 기록),
    /// 사망 중 상체 레이어 끄기, 사망 애니메이션 뒤 숨김.
    /// 구현 브랜치: feat/hero-movement, feat/hero-combat, feat/core-siege (무적 깜빡임, 사망 시 숨김), feat/vfx, feat/animation
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
            // 효과는 히어로 View가 없어도 기록 위치에 낸다
            PresentEffects(world, viewContext);

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
            float movementSqrDistance = movement.sqrMagnitude;
            if (movementSqrDistance > MIN_FACING_SQR_DISTANCE)
            {
                heroTransform.SetPositionAndRotation(position, Quaternion.LookRotation(movement));
            }
            else
            {
                heroTransform.position = position;
            }

            heroView.SetVisible(IsHeroVisible(in hero, heroView));
            PresentAnimation(world, viewContext, heroView, movementSqrDistance);
        }

        /// <summary>
        /// 이동 블렌드(이번 틱 이동 거리 ÷ 이동 속도, DESIGN 5.12)와 히어로 기록 애니메이션을 반영한다.
        /// 일시정지(레벨업 선택 대기 포함) 중에는 애니메이션을 멈춘다.
        /// 패배는 마지막 목숨을 잃은 틱에 바로 오므로 사망 애니메이션을 끝까지 재생하도록 멈추지 않는다.
        /// 월드가 멈춘 동안은 틱이 진행되지 않아 이동 거리가 마지막 값에 남아 있으므로 이동 블렌드는 0으로 둔다.
        /// 사망 중에는 상체 레이어를 꺼 사망 자세를 덮지 않게 한다.
        /// </summary>
        private static void PresentAnimation(World world, ViewContext viewContext, HeroView heroView, float movementSqrDistance)
        {
            ref readonly Hero hero = ref world.hero;
            bool isDead = hero.state == HeroState.Dead;
            heroView.SetAnimationPaused(world.pauseReason != PauseReason.None);
            heroView.SetUpperBodyEnabled(!isDead);

            bool isMoving = !ViewInterpolation.IsSimulationStopped(world)
                && hero.state == HeroState.Moving
                && movementSqrDistance > MIN_FACING_SQR_DISTANCE;
            float speedRatio = isMoving && hero.moveSpeed > 0f
                ? Mathf.Clamp01(Mathf.Sqrt(movementSqrDistance) / hero.moveSpeed)
                : 0f;
            heroView.SetMoveSpeedRatio(speedRatio);

            if (world.heroDiedRecords.Count > 0)
            {
                heroView.PlayDie();
            }
            if (world.heroRespawnedRecords.Count > 0)
            {
                heroView.PlayRespawn();
            }

            if (isDead)
            {
                return;
            }

            if (world.heroDamagedRecords.Count > 0)
            {
                heroView.PlayHit();
            }
            if (world.heroMeleeAttackRecords.Count > 0)
            {
                // 근거리 공격마다 동작을 차례로 바꾼다 (Attack01 → 02 → 03 → 01 ...)
                int variationCount = Mathf.Max(1, heroView.MeleeAttackVariationCount);
                heroView.PlayMeleeAttack(viewContext.heroMeleeAttackCount % variationCount);
                viewContext.heroMeleeAttackCount = (viewContext.heroMeleeAttackCount + 1) % variationCount;
            }
        }

        /// <summary>
        /// 히어로 기록마다 효과를 낸다. 근거리 범위는 공격 반지름만큼 키운다 (프리팹은 반지름 1 크기).
        /// </summary>
        private static void PresentEffects(World world, ViewContext viewContext)
        {
            List<HeroMeleeAttackRecord> meleeRecords = world.heroMeleeAttackRecords;
            for (int recordIndex = 0; recordIndex < meleeRecords.Count; recordIndex++)
            {
                HeroMeleeAttackRecord meleeRecord = meleeRecords[recordIndex];
                VisualEffectViewSystem.Spawn(viewContext, VisualEffectKind.HeroMeleeRange, meleeRecord.position, meleeRecord.radius);
            }

            List<HeroDamagedRecord> damagedRecords = world.heroDamagedRecords;
            for (int recordIndex = 0; recordIndex < damagedRecords.Count; recordIndex++)
            {
                VisualEffectViewSystem.Spawn(viewContext, VisualEffectKind.HeroHit, damagedRecords[recordIndex].position, 1f);
            }

            List<HeroDiedRecord> diedRecords = world.heroDiedRecords;
            for (int recordIndex = 0; recordIndex < diedRecords.Count; recordIndex++)
            {
                VisualEffectViewSystem.Spawn(viewContext, VisualEffectKind.HeroDied, diedRecords[recordIndex].position, 1f);
            }

            List<HeroRespawnedRecord> respawnedRecords = world.heroRespawnedRecords;
            for (int recordIndex = 0; recordIndex < respawnedRecords.Count; recordIndex++)
            {
                VisualEffectViewSystem.Spawn(viewContext, VisualEffectKind.HeroRespawned, respawnedRecords[recordIndex].position, 1f);
            }
        }

        /// <summary>
        /// 부활 대기 중에는 사망 애니메이션 시간(DeathVisibleTicks)이 지나면 숨기고,
        /// 피격 무적 중에는 남은 무적 틱으로 깜빡인다 (DESIGN 3.1: 깜빡임은 View가 표현).
        /// 남은 틱으로 계산하므로 일시정지 중에는 깜빡임과 사망 표시 시간도 멈춘다.
        /// 목숨이 없으면 부활 대기 틱이 줄지 않으므로 사망 자세 그대로 남는다.
        /// </summary>
        private static bool IsHeroVisible(in Hero hero, HeroView heroView)
        {
            if (hero.state == HeroState.Dead)
            {
                int deadTicks = hero.respawnTicks - hero.respawnRemainingTicks;
                return deadTicks < heroView.DeathVisibleTicks;
            }
            int blinkTicks = heroView.InvincibilityBlinkTicks;
            if (hero.invincibilityRemainingTicks <= 0)
            {
                return true;
            }
            return (hero.invincibilityRemainingTicks / Mathf.Max(1, blinkTicks)) % 2 == 0;
        }
    }
}
