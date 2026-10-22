using System.Collections.Generic;

namespace Project05.Component.Input
{
    /// <summary>
    /// 입력 버퍼. Runner가 Update에서 쌓고 CommandSystem이 FixedTick에서 소비한다.
    /// </summary>
    public class InputBuffer
    {
        // 아직 처리하지 않은 명령 (먼저 들어온 순서대로 처리)
        public readonly Queue<Command> commands = new Queue<Command>(INITIAL_CAPACITY);

        private const int INITIAL_CAPACITY = 16;
    }
}
