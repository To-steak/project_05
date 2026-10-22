#if UNITY_EDITOR || DEVELOPMENT_BUILD
using Project05.Component;
using Project05.Component.Data;
using Project05.Component.Input;
using Project05.System;
using UnityEngine;

namespace Project05.Core
{
    /// <summary>
    /// 성장 상태와 레벨업 선택지를 보여 주는 OnGUI 디버그 패널. 레벨업 UI(View-Presenter)가 생기기 전까지 대신 쓴다.
    /// Runner.OnGUI가 호출한다. 버튼은 UI 이벤트 예외로 다루며 World를 직접 바꾸지 않고 world.input에 명령만 넣는다.
    /// </summary>
    public static class ProgressDebugPanel
    {
        private const float PANEL_X = 340f;
        private const float PANEL_Y = 10f;
        private const float PANEL_WIDTH = 300f;
        private const float PANEL_HEIGHT = 260f;

        /// <summary>
        /// 패널을 그리고 선택지 버튼 입력을 명령으로 넣는다.
        /// </summary>
        /// <param name="world">현재 판의 월드</param>
        public static void Draw(World world)
        {
            ref ProgressState progress = ref world.progress;

            GUILayout.BeginArea(new Rect(PANEL_X, PANEL_Y, PANEL_WIDTH, PANEL_HEIGHT), GUI.skin.box);
            GUILayout.Label("[Progress Debug]");
            GUILayout.Label("Level: " + progress.level + "  XP: " + progress.currentXP + " / " + ProgressSystem.GetRequiredXP(world, progress.level));
            GUILayout.Label("Gold: " + progress.gold);
            GUILayout.Label("Gain: Gold " + progress.goldGainPercent + "%  XP " + progress.xpGainPercent + "%");
            GUILayout.Label("Pending Level Up: " + progress.pendingLevelUpCount + "  Drops: " + world.drops.Count);

            if (world.pauseReason == PauseReason.LevelUpChoice)
            {
                GUILayout.Label("레벨업! 선택지를 고르세요");
                DrawOptionButtons(world);
            }

            GUILayout.EndArea();
        }

        private static void DrawOptionButtons(World world)
        {
            int[] offeredOptionIndices = world.progress.offeredOptionIndices;
            for (int offeredSlot = 0; offeredSlot < offeredOptionIndices.Length; offeredSlot++)
            {
                int optionIndex = offeredOptionIndices[offeredSlot];
                if (optionIndex == ProgressSystem.NO_OPTION)
                {
                    continue;
                }

                ref LevelUpOptionStats option = ref world.levelUpOptions[optionIndex];
                string label = (offeredSlot + 1) + ". " + GetEffectName(option.effectKind) + " +" + option.amount + GetAmountUnit(option.effectKind);
                if (GUILayout.Button(label))
                {
                    Command command = new Command();
                    command.kind = CommandKind.ChooseLevelUpOption;
                    command.optionIndex = offeredSlot;
                    world.input.commands.Enqueue(command);
                }
            }
        }

        private static string GetEffectName(LevelUpEffectKind effectKind)
        {
            switch (effectKind)
            {
                case LevelUpEffectKind.HeroMaxHP:
                    return "캐릭터 HP";
                case LevelUpEffectKind.CoreMaxHP:
                    return "코어 HP";
                case LevelUpEffectKind.HeroAttackDamage:
                    return "캐릭터 공격력";
                case LevelUpEffectKind.GoldGainPercent:
                    return "골드 획득량";
                case LevelUpEffectKind.XPGainPercent:
                    return "경험치 획득량";
                default:
                    return "알 수 없음";
            }
        }

        private static string GetAmountUnit(LevelUpEffectKind effectKind)
        {
            if (effectKind == LevelUpEffectKind.GoldGainPercent || effectKind == LevelUpEffectKind.XPGainPercent)
            {
                return "%";
            }
            return "";
        }
    }
}
#endif
