using Project05.Component;
using Project05.Component.Data;
using Project05.Component.Entity;

namespace Project05.System
{
    /// <summary>
    /// [Tick 없음] XP 증가, 레벨업(일시정지), 선택지 3개 뽑기, 효과 적용. Tick 없이 다른 System이 직접 호출한다.
    /// 레벨업 대기가 있으면 world.pauseReason을 LevelUpChoice로 바꿔 CommandSystem만 돌게 하고,
    /// 선택 명령이 들어오면 효과를 적용한 뒤 남은 대기가 있으면 새 선택지를 바로 제시한다 (DESIGN 3.6).
    /// 구현 브랜치: feat/drop-progress
    /// </summary>
    public static class ProgressSystem
    {
        /// <summary>
        /// XP를 더하고 필요 XP를 넘은 만큼 레벨을 올린다. 레벨업 대기가 생기면 선택지를 제시하고 일시정지한다.
        /// 남은 XP는 다음 레벨로 넘어가며, 한 번에 여러 레벨이 오르면 대기 수만큼 연속으로 선택한다.
        /// 획득 퍼센트는 호출하는 쪽에서 이미 적용한 값을 넘긴다.
        /// </summary>
        /// <param name="world">현재 판의 월드</param>
        /// <param name="amount">더할 XP (0 이하면 무시)</param>
        public static void AddXP(World world, int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            ref ProgressState progress = ref world.progress;
            progress.currentXP += amount;

            // 필요 XP가 0 이하(테이블 없음·잘못된 값)이면 레벨업하지 않는다 (무한 루프 방지)
            int requiredXP = GetRequiredXP(world, progress.level);
            while (requiredXP > 0 && progress.currentXP >= requiredXP)
            {
                progress.currentXP -= requiredXP;
                progress.level++;
                progress.pendingLevelUpCount++;
                requiredXP = GetRequiredXP(world, progress.level);
            }

            TryOpenLevelUpChoice(world);
        }

        /// <summary>
        /// 해당 레벨에서 다음 레벨까지 필요한 XP를 구한다. 테이블보다 높은 레벨은 마지막 값을 계속 쓴다.
        /// </summary>
        /// <param name="world">현재 판의 월드</param>
        /// <param name="level">현재 레벨 (1부터)</param>
        /// <returns>필요 XP. 테이블이 비어 있으면 0 (레벨업 없음)</returns>
        public static int GetRequiredXP(World world, int level)
        {
            int[] requiredXPByLevel = world.progression.requiredXPByLevel;
            if (requiredXPByLevel == null || requiredXPByLevel.Length == 0)
            {
                return 0;
            }

            int tableIndex = level - 1;
            if (tableIndex < 0)
            {
                tableIndex = 0;
            }
            if (tableIndex >= requiredXPByLevel.Length)
            {
                tableIndex = requiredXPByLevel.Length - 1;
            }
            return requiredXPByLevel[tableIndex];
        }

        /// <summary>
        /// 레벨업 대기가 있고 다른 일시정지가 없으면 선택지를 뽑아 제시하고 LevelUpChoice로 일시정지한다.
        /// 코어 UI를 닫을 때(feat/core-menu)도 호출해 밀린 레벨업을 이어서 제시한다.
        /// </summary>
        /// <param name="world">현재 판의 월드</param>
        /// <returns>선택지를 제시하고 일시정지했으면 true</returns>
        public static bool TryOpenLevelUpChoice(World world)
        {
            if (world.pauseReason != PauseReason.None)
            {
                return false;
            }
            if (world.progress.pendingLevelUpCount <= 0)
            {
                return false;
            }

            // 선택지 테이블이 비어 있으면 제시할 것이 없으므로 멈추지 않는다 (대기 수는 남겨 둔다)
            if (!OfferOptions(world))
            {
                return false;
            }

            world.pauseReason = PauseReason.LevelUpChoice;
            return true;
        }

        /// <summary>
        /// 제시된 선택지 중 하나를 골라 효과를 적용한다. 레벨업 선택 중이 아니거나 빈 칸이면 아무것도 하지 않는다.
        /// 적용 후 대기 수를 줄이고 일시정지를 풀며, 대기가 남아 있으면 새 선택지를 바로 제시한다.
        /// </summary>
        /// <param name="world">현재 판의 월드</param>
        /// <param name="offeredSlot">고른 제시 칸 번호 (0 ~ OFFERED_OPTION_COUNT - 1)</param>
        /// <returns>선택이 적용됐으면 true</returns>
        public static bool ChooseLevelUpOption(World world, int offeredSlot)
        {
            if (world.pauseReason != PauseReason.LevelUpChoice)
            {
                return false;
            }

            ref ProgressState progress = ref world.progress;
            int[] offeredOptionIndices = progress.offeredOptionIndices;
            if (offeredSlot < 0 || offeredSlot >= offeredOptionIndices.Length)
            {
                return false;
            }

            int optionIndex = offeredOptionIndices[offeredSlot];
            if (optionIndex == ProgressState.NO_OFFERED_OPTION)
            {
                return false;
            }

            ref LevelUpOptionStats option = ref world.levelUpOptions[optionIndex];
            ApplyOption(world, in option);

            progress.pendingLevelUpCount--;
            ClearOfferedOptions(offeredOptionIndices);
            world.pauseReason = PauseReason.None;

            TryOpenLevelUpChoice(world);
            return true;
        }

        /// <summary>
        /// 선택지 테이블에서 제시 칸 수만큼 서로 다른 선택지를 시드 고정 랜덤으로 뽑는다.
        /// 테이블이 제시 칸보다 작으면 테이블 전체를 제시하고 나머지 칸은 비운다.
        /// </summary>
        /// <returns>하나 이상 제시했으면 true</returns>
        private static bool OfferOptions(World world)
        {
            LevelUpOptionStats[] levelUpOptions = world.levelUpOptions;
            int optionCount = levelUpOptions == null ? 0 : levelUpOptions.Length;
            int[] offeredOptionIndices = world.progress.offeredOptionIndices;

            ClearOfferedOptions(offeredOptionIndices);

            int offerCount = offeredOptionIndices.Length < optionCount ? offeredOptionIndices.Length : optionCount;
            for (int offeredSlot = 0; offeredSlot < offerCount; offeredSlot++)
            {
                // 선택지 수가 적으므로 중복이면 다시 뽑는다 (할당 없음)
                int candidateIndex = world.random.Next(optionCount);
                while (IsAlreadyOffered(offeredOptionIndices, offeredSlot, candidateIndex))
                {
                    candidateIndex = world.random.Next(optionCount);
                }
                offeredOptionIndices[offeredSlot] = candidateIndex;
            }

            return offerCount > 0;
        }

        private static bool IsAlreadyOffered(int[] offeredOptionIndices, int filledCount, int optionIndex)
        {
            for (int offeredSlot = 0; offeredSlot < filledCount; offeredSlot++)
            {
                if (offeredOptionIndices[offeredSlot] == optionIndex)
                {
                    return true;
                }
            }
            return false;
        }

        private static void ClearOfferedOptions(int[] offeredOptionIndices)
        {
            for (int offeredSlot = 0; offeredSlot < offeredOptionIndices.Length; offeredSlot++)
            {
                offeredOptionIndices[offeredSlot] = ProgressState.NO_OFFERED_OPTION;
            }
        }

        private static void ApplyOption(World world, in LevelUpOptionStats option)
        {
            switch (option.effectKind)
            {
                case LevelUpEffectKind.HeroMaxHP:
                {
                    ref Hero hero = ref world.hero;
                    hero.maxHP += option.amount;
                    // 부활 대기 중이면 HP를 채우지 않는다. 부활 시 최대 HP로 채워진다 (DESIGN 3.1)
                    if (hero.state != HeroState.Dead)
                    {
                        hero.currentHP = hero.maxHP;
                    }
                    break;
                }
                case LevelUpEffectKind.CoreMaxHP:
                {
                    ref CoreState core = ref world.core;
                    core.maxHP += option.amount;
                    core.currentHP = core.maxHP;
                    break;
                }
                case LevelUpEffectKind.HeroMeleeAttackDamage:
                    world.hero.meleeAttackDamage += option.amount;
                    break;
                case LevelUpEffectKind.HeroRangedAttackDamage:
                    world.hero.rangedAttackDamage += option.amount;
                    break;
                case LevelUpEffectKind.GoldGainPercent:
                    world.progress.goldGainPercent += option.amount;
                    break;
                case LevelUpEffectKind.XPGainPercent:
                    world.progress.xpGainPercent += option.amount;
                    break;
            }
        }
    }
}
