namespace Project05.Component
{
    /// <summary>
    /// 일시정지 사유. None이 아니면 CommandSystem만 실행한다.
    /// </summary>
    public enum PauseReason
    {
        // 일시정지 아님
        None,
        // 레벨업 선택지 표시 중
        LevelUpChoice,
        // 코어 UI 열림
        CoreMenu,
    }
}
