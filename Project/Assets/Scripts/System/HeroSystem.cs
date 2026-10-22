using Project05.Component;
using Project05.Component.Entity;
using UnityEngine;

namespace Project05.System
{
    /// <summary>
    /// [Tick 4] 히어로 이동, 탑승/하차 상태, 공격 타이머 1개로 근거리 광역 / 원거리 자동 공격, 접촉 피해와 무적, 사망·목숨·부활을 처리한다.
    /// 구현 브랜치: feat/hero-movement, feat/hero-combat, feat/core-siege
    /// </summary>
    public static class HeroSystem
    {
        /// <summary>
        /// 히어로를 한 틱 진행한다.
        /// </summary>
        /// <param name="world">현재 판의 월드</param>
        public static void Tick(World world)
        {
            ref Hero hero = ref world.hero;

            // View 보간 기준점. 움직이지 않는 틱에도 갱신해야 이전 이동이 반복 보간되지 않는다
            hero.previousPosition = hero.currentPosition;

            if (hero.state == HeroState.Moving)
            {
                MoveAlongPath(ref hero);
            }
        }

        /// <summary>
        /// 이번 틱의 이동 거리(moveSpeed)만큼 경로 코너를 따라간다. 한 틱에 코너를 여러 개 지날 수 있다.
        /// 마지막 코너에 닿으면 Idle로 바꾼다.
        /// </summary>
        private static void MoveAlongPath(ref Hero hero)
        {
            float remainingDistance = hero.moveSpeed;
            Vector3 position = hero.currentPosition;

            while (remainingDistance > 0f && hero.nextPathCornerIndex < hero.pathCornerCount)
            {
                ref Vector3 corner = ref hero.pathCorners[hero.nextPathCornerIndex];
                Vector3 toCorner = corner - position;
                float sqrDistanceToCorner = toCorner.sqrMagnitude;

                if (sqrDistanceToCorner <= remainingDistance * remainingDistance)
                {
                    position = corner;
                    remainingDistance -= Mathf.Sqrt(sqrDistanceToCorner);
                    hero.nextPathCornerIndex++;
                }
                else
                {
                    position += toCorner * (remainingDistance / Mathf.Sqrt(sqrDistanceToCorner));
                    remainingDistance = 0f;
                }
            }

            hero.currentPosition = position;

            if (hero.nextPathCornerIndex >= hero.pathCornerCount)
            {
                hero.state = HeroState.Idle;
                hero.pathCornerCount = 0;
                hero.nextPathCornerIndex = 0;
            }
        }
    }
}
