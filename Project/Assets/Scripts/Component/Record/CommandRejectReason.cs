namespace Project05.Component.Record
{
    /// <summary>
    /// 명령 거절 사유.
    /// </summary>
    public enum CommandRejectReason
    {
        // 골드 부족
        NotEnoughGold,
        // 상호작용 범위 밖
        OutOfRange,
        // 해금되지 않은 타워 종류
        NotUnlocked,
        // 이미 타워가 있거나 건설 대기 중인 공터
        SlotOccupied,
        // 이미 해금된 타워 종류
        AlreadyUnlocked,
        // 이미 최대 단계인 타워
        MaxLevel,
        // 이미 체력이 가득 찬 타워
        AlreadyFullHP,
        // 대상 공터에 타워가 없음
        NoTower,
    }
}
