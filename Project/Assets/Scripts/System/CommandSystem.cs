using System.Collections.Generic;
using Project05.Component;
using Project05.Component.Entity;
using Project05.Component.Input;
using Project05.Component.Record;
using Project05.Component.Request;
using UnityEngine;
using UnityEngine.AI;

namespace Project05.System
{
    /// <summary>
    /// [Tick 1] 입력 버퍼를 소비해 이동·탑승·하차·사격·건설·코어 UI 명령으로 해석한다. 일시정지 중에도 실행되며, 이때는 열린 UI의 명령만 처리하고 나머지는 버린다.
    /// 구현 브랜치: feat/hero-movement (이동), feat/enemy-wave (디버그 웨이브 클리어), feat/tower (Interact 건설 목록·건설, Fire), feat/drop-progress (레벨업 선택), feat/core-menu (코어 UI 명령)
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

            // 한 틱에 레벨업 선택은 한 번만 받는다. 연타가 아직 보지 않은 다음 선택지를 고르지 않게 한다
            bool hasChosenLevelUpOption = false;

            while (commands.Count > 0)
            {
                Command command = commands.Dequeue();

                // 일시정지 중에는 열린 UI의 명령만 처리하고 나머지는 버린다
                if (world.pauseReason != PauseReason.None && !IsAllowedWhilePaused(world.pauseReason, command.kind))
                {
                    continue;
                }

                switch (command.kind)
                {
                    case CommandKind.MoveTo:
                        MoveHero(world, command.groundPosition);
                        break;
                    case CommandKind.ChooseLevelUpOption:
                        if (!hasChosenLevelUpOption)
                        {
                            hasChosenLevelUpOption = ProgressSystem.ChooseLevelUpOption(world, command.optionIndex);
                        }
                        break;
                    case CommandKind.Interact:
                        Interact(world);
                        break;
                    case CommandKind.BuildTower:
                        BuildTower(world, command.towerTypeIndex);
                        break;
                    case CommandKind.CloseBuildMenu:
                        CloseBuildMenu(world);
                        break;
                    case CommandKind.DebugClearWave:
                        GameFlowSystem.ClearWave(world);
                        break;
                    default:
                        // Fire(탑승 사격), 코어 UI 명령은 담당 브랜치에서 구현한다
                        break;
                }
            }
        }

        /// <summary>
        /// 일시정지 사유(열린 UI)마다 받는 명령만 통과시킨다. 건설 목록에서는 Space(Interact)가 닫기다.
        /// </summary>
        private static bool IsAllowedWhilePaused(PauseReason pauseReason, CommandKind kind)
        {
            switch (pauseReason)
            {
                case PauseReason.LevelUpChoice:
                    return kind == CommandKind.ChooseLevelUpOption;
                case PauseReason.BuildMenu:
                    return kind == CommandKind.BuildTower || kind == CommandKind.CloseBuildMenu || kind == CommandKind.Interact;
                case PauseReason.CoreMenu:
                    return kind == CommandKind.UnlockTowerType
                        || kind == CommandKind.UpgradeTower
                        || kind == CommandKind.RepairTower
                        || kind == CommandKind.CloseCoreMenu;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Space 상호작용. 건설 목록이 열려 있으면 닫는다.
        /// 지상(Idle, Moving)에서 상호작용 반경 안에 빈 공터가 있으면 건설 목록을 열고 일시정지한다 (가장 가까운 공터).
        /// 타워 탑승(feat/tower 후속)과 코어 UI(feat/core-menu)는 담당 브랜치에서 붙인다.
        /// </summary>
        private static void Interact(World world)
        {
            if (world.pauseReason == PauseReason.BuildMenu)
            {
                CloseBuildMenu(world);
                return;
            }

            ref Hero hero = ref world.hero;
            if (hero.state != HeroState.Idle && hero.state != HeroState.Moving)
            {
                return;
            }

            int slotIndex = FindNearestSlotInReach(world, hero.currentPosition, hero.interactionRadius);
            if (slotIndex == World.NO_SLOT)
            {
                return;
            }

            ref TowerSlot slot = ref world.towerSlots[slotIndex];
            if (slot.towerID.IsValid || slot.isBuildPending)
            {
                // 타워가 있는 공터: 탑승은 feat/tower 후속 PR에서
                return;
            }

            // 목록을 여는 순간 히어로를 세운다 (닫은 뒤 공터에서 멀어지지 않도록)
            StopHero(ref hero);
            world.buildMenuSlotIndex = slotIndex;
            world.pauseReason = PauseReason.BuildMenu;
        }

        /// <summary>
        /// 건설 목록이 열린 공터에 타워를 짓는다. 해금·빈 공터·골드를 확인하고 비용을 낸 뒤 건설 요청을 넣고 목록을 닫는다.
        /// 거절되면 CommandRejectedRecord를 남기고 목록은 열어 둔다.
        /// </summary>
        private static void BuildTower(World world, int towerTypeIndex)
        {
            int slotIndex = world.buildMenuSlotIndex;
            if (world.pauseReason != PauseReason.BuildMenu || slotIndex < 0 || slotIndex >= world.towerSlots.Length)
            {
                return;
            }
            if (towerTypeIndex < 0 || towerTypeIndex >= world.towerStats.Length || world.towerStats[towerTypeIndex].levels.Length == 0)
            {
                return;
            }
            if (!world.towerUnlock.isUnlockedByTowerType[towerTypeIndex])
            {
                Reject(world, CommandKind.BuildTower, CommandRejectReason.NotUnlocked);
                return;
            }

            ref TowerSlot slot = ref world.towerSlots[slotIndex];
            if (slot.towerID.IsValid || slot.isBuildPending)
            {
                Reject(world, CommandKind.BuildTower, CommandRejectReason.SlotOccupied);
                return;
            }
            if (!EconomySystem.TrySpendGold(world, world.towerStats[towerTypeIndex].buildCost))
            {
                Reject(world, CommandKind.BuildTower, CommandRejectReason.NotEnoughGold);
                return;
            }

            TowerBuildRequest request = new TowerBuildRequest();
            request.slotIndex = slotIndex;
            request.towerTypeIndex = towerTypeIndex;
            world.towerBuildRequests.Enqueue(request);
            slot.isBuildPending = true;

            CloseBuildMenu(world);
        }

        /// <summary>
        /// 건설 목록을 닫고 일시정지를 푼다. 목록 때문에 미뤄진 레벨업 선택이 있으면 바로 연다.
        /// </summary>
        private static void CloseBuildMenu(World world)
        {
            if (world.pauseReason != PauseReason.BuildMenu)
            {
                return;
            }

            world.buildMenuSlotIndex = World.NO_SLOT;
            world.pauseReason = PauseReason.None;
            ProgressSystem.TryOpenLevelUpChoice(world);
        }

        /// <summary>
        /// 위치에서 반경 이내(XZ)의 가장 가까운 공터를 찾는다.
        /// </summary>
        private static int FindNearestSlotInReach(World world, Vector3 position, float radius)
        {
            int nearestSlotIndex = World.NO_SLOT;
            float nearestSqrDistance = radius * radius;
            for (int slotIndex = 0; slotIndex < world.towerSlots.Length; slotIndex++)
            {
                float sqrDistance = TargetingSystem.GetSqrDistanceXZ(position, world.towerSlots[slotIndex].position);
                if (sqrDistance <= nearestSqrDistance)
                {
                    nearestSqrDistance = sqrDistance;
                    nearestSlotIndex = slotIndex;
                }
            }
            return nearestSlotIndex;
        }

        private static void Reject(World world, CommandKind commandKind, CommandRejectReason reason)
        {
            CommandRejectedRecord rejectedRecord = new CommandRejectedRecord();
            rejectedRecord.commandKind = commandKind;
            rejectedRecord.reason = reason;
            world.commandRejectedRecords.Add(rejectedRecord);
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
