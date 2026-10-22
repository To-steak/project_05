namespace Project05.Component.Input
{
    /// <summary>
    /// 입력 명령 종류.
    /// </summary>
    public enum CommandKind
    {
        // 지면 좌표로 이동 (지상 좌클릭)
        MoveTo,
        // Space: 대상에 따라 건설 / 탑승 / 하차 / 코어 UI 열기
        Interact,
        // 탑승 중 사격 (탑승 중 좌클릭)
        Fire,
        // 레벨업 선택지 고르기
        ChooseLevelUpOption,
        // 코어 UI: 타워 종류 해금
        UnlockTowerType,
        // 코어 UI: 타워 업그레이드
        UpgradeTower,
        // 코어 UI: 타워 수리
        RepairTower,
        // 코어 UI 닫기
        CloseCoreMenu,
    }
}
