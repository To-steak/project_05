using Project05.Component.Input;

namespace Project05.Component.Record
{
    /// <summary>
    /// 프레임 기록: 명령 거절 (안내 문구, 실패 효과음).
    /// </summary>
    public struct CommandRejectedRecord
    {
        // 거절된 명령 종류
        public CommandKind commandKind;
        // 거절 사유
        public CommandRejectReason reason;
    }
}
