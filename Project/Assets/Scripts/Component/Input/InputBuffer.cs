using System.Collections.Generic;
using UnityEngine;

namespace Project05.Component.Input
{
    /// <summary>
    /// 입력 버퍼. Runner가 Update에서 쌓고 CommandSystem이 FixedTick에서 소비한다.
    /// 누른 순간의 일은 명령 큐로, 누르고 있는 상태(연속 입력)는 필드로 담는다.
    /// </summary>
    public class InputBuffer
    {
        // 아직 처리하지 않은 명령 (먼저 들어온 순서대로 처리)
        public readonly Queue<Command> commands = new Queue<Command>(INITIAL_CAPACITY);
        // 좌클릭을 누르고 있는지 (Runner가 매 Update 갱신하는 상태. 명령이 아니므로 소비하지 않는다)
        public bool isPointerHeld;
        // 누르고 있는 동안 커서가 가리키는 지면 좌표가 있는지 (레이캐스트 실패 시 false)
        public bool hasHeldGroundPosition;
        // 누르고 있는 동안 커서가 가리키는 지면 좌표 (지상: 이동 목표, 탑승: 사격 방향)
        public Vector3 heldGroundPosition;

        private const int INITIAL_CAPACITY = 16;
    }
}
