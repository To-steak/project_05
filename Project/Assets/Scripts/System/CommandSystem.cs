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
    /// 구현 브랜치: feat/hero-movement (이동), feat/enemy-wave (디버그 웨이브 클리어), feat/tower (Interact 건설 목록·건설·탑승), feat/drop-progress (레벨업 선택), feat/core-menu (코어 UI 해금·업그레이드·수리, 일시정지 메뉴)
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
                        ClosePause(world, PauseReason.BuildMenu);
                        break;
                    case CommandKind.UnlockTowerType:
                        UnlockTowerType(world, command.towerTypeIndex);
                        break;
                    case CommandKind.UpgradeTower:
                        UpgradeTower(world, command.slotIndex);
                        break;
                    case CommandKind.RepairTower:
                        RepairTower(world, command.slotIndex);
                        break;
                    case CommandKind.CloseCoreMenu:
                        ClosePause(world, PauseReason.CoreMenu);
                        break;
                    case CommandKind.TogglePauseMenu:
                        TogglePauseMenu(world);
                        break;
                    case CommandKind.ClosePauseMenu:
                        ClosePause(world, PauseReason.PauseMenu);
                        break;
                    case CommandKind.DebugClearWave:
                        GameFlowSystem.ClearWave(world);
                        break;
                    default:
                        break;
                }
            }

            if (world.pauseReason == PauseReason.None)
            {
                FollowHeldPointer(world);
            }

            UpdateInteractionTarget(world);
        }

        /// <summary>
        /// 좌클릭을 누르고 있는 동안(연속 입력) 지상의 히어로가 커서 지점을 계속 따라가게 한다.
        /// 목표가 moveRepathDistance 이상 바뀔 때만 경로를 다시 계산한다 (매 틱 NavMesh 계산을 피한다).
        /// 탑승 중 사격은 TowerSystem이 같은 입력 상태를 읽어 처리한다.
        /// </summary>
        private static void FollowHeldPointer(World world)
        {
            InputBuffer input = world.input;
            ref Hero hero = ref world.hero;
            if (!input.isPointerHeld || !input.hasHeldGroundPosition)
            {
                return;
            }
            if (hero.state != HeroState.Idle && hero.state != HeroState.Moving)
            {
                return;
            }
            if (TargetingSystem.GetSqrDistanceXZ(hero.moveTargetPosition, input.heldGroundPosition) < hero.moveRepathDistance * hero.moveRepathDistance)
            {
                return;
            }

            MoveHero(world, input.heldGroundPosition);
        }

        /// <summary>
        /// 일시정지 사유(열린 UI)마다 받는 명령만 통과시킨다. 건설 목록·코어 UI에서는 Space(Interact)가 닫기다.
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
                        || kind == CommandKind.CloseCoreMenu
                        || kind == CommandKind.Interact;
                case PauseReason.PauseMenu:
                    return kind == CommandKind.TogglePauseMenu || kind == CommandKind.ClosePauseMenu;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Space 상호작용. 건설 목록·코어 UI가 열려 있으면 닫고, 탑승 중이면 하차한다.
        /// 지상(Idle, Moving)에서 상호작용 반경 안의 가장 가까운 공터에 타워가 있으면 탑승, 비어 있으면 건설 목록을 연다.
        /// 공터가 없고 코어가 반경 안이면 코어 UI를 연다 (레벨 디자인으로 공터와 코어 범위는 겹치지 않는다, DESIGN 3.2).
        /// </summary>
        private static void Interact(World world)
        {
            if (world.pauseReason == PauseReason.BuildMenu || world.pauseReason == PauseReason.CoreMenu)
            {
                ClosePause(world, world.pauseReason);
                return;
            }

            int slotIndex;
            Vector3 targetPosition;
            ref Hero hero = ref world.hero;
            switch (FindInteractionTarget(world, out slotIndex, out targetPosition))
            {
                case InteractionTargetKind.BoardedTower:
                    HeroSystem.Dismount(world);
                    break;
                case InteractionTargetKind.Tower:
                    HeroSystem.Board(world, world.towerSlots[slotIndex].towerID);
                    break;
                case InteractionTargetKind.EmptySlot:
                    // 목록을 여는 순간 히어로를 세운다 (닫은 뒤 공터에서 멀어지지 않도록)
                    StopHero(ref hero);
                    world.buildMenuSlotIndex = slotIndex;
                    world.pauseReason = PauseReason.BuildMenu;
                    break;
                case InteractionTargetKind.Core:
                    StopHero(ref hero);
                    world.pauseReason = PauseReason.CoreMenu;
                    break;
            }
        }

        /// <summary>
        /// 지금 Space를 누르면 반응할 대상을 World에 기록한다 (View의 Space 아이콘이 읽는다). Interact와 같은 판정을 쓴다.
        /// 일시정지 중에는 대상을 비운다.
        /// </summary>
        private static void UpdateInteractionTarget(World world)
        {
            if (world.pauseReason != PauseReason.None)
            {
                world.interactionTargetKind = InteractionTargetKind.None;
                return;
            }

            int slotIndex;
            Vector3 targetPosition;
            world.interactionTargetKind = FindInteractionTarget(world, out slotIndex, out targetPosition);
            world.interactionTargetPosition = targetPosition;
        }

        /// <summary>
        /// Space 대상 판정. 탑승 중이면 그 타워(하차). 지상(Idle, Moving)이면 상호작용 반경 안의 가장 가까운 공터
        /// (타워 있음: 탑승, 비어 있음: 건설 목록, 건설 대기 중: 없음), 공터가 없고 코어가 반경 안이면 코어.
        /// </summary>
        private static InteractionTargetKind FindInteractionTarget(World world, out int slotIndex, out Vector3 targetPosition)
        {
            slotIndex = World.NO_SLOT;
            targetPosition = Vector3.zero;
            ref Hero hero = ref world.hero;

            if (hero.state == HeroState.Boarded)
            {
                if (!world.towers.IsAlive(hero.boardedTowerID))
                {
                    return InteractionTargetKind.None;
                }
                targetPosition = world.towers.Get(hero.boardedTowerID).position;
                return InteractionTargetKind.BoardedTower;
            }
            if (hero.state != HeroState.Idle && hero.state != HeroState.Moving)
            {
                return InteractionTargetKind.None;
            }

            slotIndex = FindNearestSlotInReach(world, hero.currentPosition, hero.interactionRadius);
            if (slotIndex == World.NO_SLOT)
            {
                if (TargetingSystem.GetSqrDistanceXZ(hero.currentPosition, world.core.position) <= hero.interactionRadius * hero.interactionRadius)
                {
                    targetPosition = world.core.position;
                    return InteractionTargetKind.Core;
                }
                return InteractionTargetKind.None;
            }

            ref TowerSlot slot = ref world.towerSlots[slotIndex];
            targetPosition = slot.position;
            if (world.towers.IsAlive(slot.towerID))
            {
                return InteractionTargetKind.Tower;
            }
            return slot.isBuildPending ? InteractionTargetKind.None : InteractionTargetKind.EmptySlot;
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

            ClosePause(world, PauseReason.BuildMenu);
        }

        /// <summary>
        /// 코어 UI: 타워 종류를 골드로 해금한다. 해금되면 건설 목록에 보인다. 해금은 타워가 파괴돼도 유지된다.
        /// </summary>
        private static void UnlockTowerType(World world, int towerTypeIndex)
        {
            bool[] isUnlockedByTowerType = world.towerUnlock.isUnlockedByTowerType;
            if (towerTypeIndex < 0 || towerTypeIndex >= isUnlockedByTowerType.Length)
            {
                return;
            }
            if (isUnlockedByTowerType[towerTypeIndex])
            {
                Reject(world, CommandKind.UnlockTowerType, CommandRejectReason.AlreadyUnlocked);
                return;
            }
            if (!EconomySystem.TrySpendGold(world, world.towerStats[towerTypeIndex].unlockCost))
            {
                Reject(world, CommandKind.UnlockTowerType, CommandRejectReason.NotEnoughGold);
                return;
            }

            isUnlockedByTowerType[towerTypeIndex] = true;
        }

        /// <summary>
        /// 코어 UI: 공터의 타워를 다음 단계로 올린다 (TowerSystem.TryUpgrade). 거절되면 사유를 기록한다.
        /// </summary>
        private static void UpgradeTower(World world, int slotIndex)
        {
            CommandRejectReason reason;
            if (!TowerSystem.TryUpgrade(world, slotIndex, out reason))
            {
                Reject(world, CommandKind.UpgradeTower, reason);
            }
        }

        /// <summary>
        /// 코어 UI: 공터의 타워 HP를 전부 회복한다 (TowerSystem.TryRepair). 거절되면 사유를 기록한다.
        /// </summary>
        private static void RepairTower(World world, int slotIndex)
        {
            CommandRejectReason reason;
            if (!TowerSystem.TryRepair(world, slotIndex, out reason))
            {
                Reject(world, CommandKind.RepairTower, reason);
            }
        }

        /// <summary>
        /// ESC: 일시정지가 아니면 일시정지 메뉴를 열고, 일시정지 메뉴가 열려 있으면 닫는다. 다른 UI가 열려 있으면 무시한다.
        /// </summary>
        private static void TogglePauseMenu(World world)
        {
            if (world.pauseReason == PauseReason.None)
            {
                world.pauseReason = PauseReason.PauseMenu;
                return;
            }
            ClosePause(world, PauseReason.PauseMenu);
        }

        /// <summary>
        /// 열려 있는 UI가 reason이면 닫고 일시정지를 푼다. 그 UI 때문에 미뤄진 레벨업 선택이 있으면 바로 연다.
        /// </summary>
        private static void ClosePause(World world, PauseReason reason)
        {
            if (world.pauseReason != reason)
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

            // 경로를 못 찾아도 같은 지점으로 매 틱 다시 계산하지 않도록 목표는 먼저 기록한다
            hero.moveTargetPosition = destination;

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
