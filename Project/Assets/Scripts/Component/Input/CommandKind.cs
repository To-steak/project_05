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
        // 건설 목록: 열린 공터에 타워 건설 (towerTypeIndex)
        BuildTower,
        // 건설 목록 닫기
        CloseBuildMenu,
        // [디버그] 현재 웨이브를 즉시 클리어 (처치 판정이 생기기 전 웨이브 전환 확인용, Runner 디버그 패널에서만 넣는다)
        DebugClearWave,
    }
}
