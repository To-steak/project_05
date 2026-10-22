using System.Collections.Generic;
using Project05.Component;
using Project05.Component.Entity;
using Project05.Component.Input;
using UnityEngine;
using UnityEngine.AI;

namespace Project05.System
{
    /// <summary>
    /// [Tick 1] 입력 버퍼를 소비해 이동·탑승·하차·사격·건설·코어 UI 명령으로 해석한다. 일시정지 중에도 실행되며, 이때는 열린 UI의 명령만 처리하고 나머지는 버린다.
    /// 구현 브랜치: feat/hero-movement (이동), feat/tower (Interact·Fire), feat/drop-progress·feat/core-menu (UI 명령)
    /// </summary>
    public static class CommandSystem
    {
        // NavMesh.SamplePosition 탐색 반경. 클릭 지점·히어로 위치를 가장 가까운 NavMesh 위로 옮길 때 이 거리까지 찾는다
        private const float NAVIGATION_SAMPLE_DISTANCE = 2f;
        // 이동으로 인정하는 최소 경로 코너 수 (시작 코너 + 목적지 코너)
        private const int MIN_MOVE_PATH_CORNER_COUNT = 2;

        /// <summary>
        /// 입력 버퍼의 명령을 모두 꺼내 처리한다.
        /// </summary>
        /// <param name="world">현재 판의 월드</param>
        public static void Tick(World world)
        {
            Queue<Command> commands = world.input.commands;
            while (commands.Count > 0)
            {
                Command command = commands.Dequeue();

                // 일시정지 중에는 UI 명령만 처리한다. 어떤 UI가 열렸는지에 따른 세부 거르기는 UI 명령 구현 브랜치에서 한다
                if (world.pauseReason != PauseReason.None && !IsMenuCommand(command.kind))
                {
                    continue;
                }

                switch (command.kind)
                {
                    case CommandKind.MoveTo:
                        MoveHero(world, command.groundPosition);
                        break;
                    default:
                        // Interact, Fire, UI 명령은 담당 브랜치에서 구현한다
                        break;
                }
            }
        }

        private static bool IsMenuCommand(CommandKind kind)
        {
            switch (kind)
            {
                case CommandKind.ChooseLevelUpOption:
                case CommandKind.UnlockTowerType:
                case CommandKind.UpgradeTower:
                case CommandKind.RepairTower:
                case CommandKind.CloseCoreMenu:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// NavMesh 경로를 계산해 히어로 경로 코너에 저장한다. 실제 이동은 HeroSystem이 한다.
        /// 지상(Idle, Moving)에서만 받는다. 경로가 없으면 명령을 버리고 기존 이동을 유지한다.
        /// </summary>
        private static void MoveHero(World world, Vector3 destination)
        {
            ref Hero hero = ref world.hero;
            if (hero.state != HeroState.Idle && hero.state != HeroState.Moving)
            {
                return;
            }

            NavMeshHit startHit;
            if (!NavMesh.SamplePosition(hero.currentPosition, out startHit, NAVIGATION_SAMPLE_DISTANCE, NavMesh.AllAreas))
            {
                return;
            }

            NavMeshHit destinationHit;
            if (!NavMesh.SamplePosition(destination, out destinationHit, NAVIGATION_SAMPLE_DISTANCE, NavMesh.AllAreas))
            {
                return;
            }

            // 클릭 때만 만들므로 매 틱 할당은 아니다. PathPartial이면 갈 수 있는 곳까지 간다
            NavMeshPath path = new NavMeshPath();
            if (!NavMesh.CalculatePath(startHit.position, destinationHit.position, NavMesh.AllAreas, path))
            {
                return;
            }

            int cornerCount = path.GetCornersNonAlloc(hero.pathCorners);
            if (cornerCount < MIN_MOVE_PATH_CORNER_COUNT)
            {
                StopHero(ref hero);
                return;
            }

            // 0번 코너는 시작 위치이므로 1번부터 따라간다
            hero.pathCornerCount = cornerCount;
            hero.nextPathCornerIndex = 1;
            hero.state = HeroState.Moving;
        }

        private static void StopHero(ref Hero hero)
        {
            hero.state = HeroState.Idle;
            hero.pathCornerCount = 0;
            hero.nextPathCornerIndex = 0;
        }
    }
}
