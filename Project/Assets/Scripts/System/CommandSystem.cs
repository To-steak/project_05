using System.Collections.Generic;
using Project05.Component;
using Project05.Component.Input;

namespace Project05.System
{
    /// <summary>
    /// [Tick 1] 입력 버퍼를 소비해 이동·탑승·하차·사격·건설·코어 UI 명령으로 해석한다. 일시정지 중에도 실행되며, 이때는 열린 UI의 명령만 처리하고 나머지는 버린다.
    /// 구현 브랜치: feat/hero-movement(이동·Interact), feat/drop-progress(레벨업 선택), feat/tower·feat/core-menu(사격·코어 UI)
    /// </summary>
    public static class CommandSystem
    {
        /// <summary>
        /// 입력 버퍼의 명령을 모두 꺼내 처리한다. 처리하지 않는 명령은 버린다.
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
                switch (command.kind)
                {
                    case CommandKind.ChooseLevelUpOption:
                        if (!hasChosenLevelUpOption)
                        {
                            hasChosenLevelUpOption = ProgressSystem.ChooseLevelUpOption(world, command.optionIndex);
                        }
                        break;
                }
            }
        }
    }
}
