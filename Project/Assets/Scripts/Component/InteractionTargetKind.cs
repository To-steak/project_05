namespace Project05.Component
{
    /// <summary>
    /// 지금 Space를 누르면 반응할 상호작용 대상. CommandSystem이 매 틱 계산해 World에 둔다 (View의 Space 아이콘이 읽는다).
    /// </summary>
    public enum InteractionTargetKind
    {
        // 반응할 대상이 없음
        None,
        // 빈 공터: 건설 목록 열기
        EmptySlot,
        // 타워가 있는 공터: 탑승
        Tower,
        // 탑승 중인 타워: 하차
        BoardedTower,
        // 코어: 코어 UI 열기
        Core,
    }
}
