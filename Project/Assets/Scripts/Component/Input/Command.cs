using UnityEngine;

namespace Project05.Component.Input
{
    /// <summary>
    /// 입력 명령 하나. 종류에 따라 필요한 인자만 쓴다.
    /// </summary>
    public struct Command
    {
        // 명령 종류
        public CommandKind kind;
        // 클릭한 지면 좌표 (MoveTo)
        public Vector3 groundPosition;
        // 대상 공터 인덱스 (UpgradeTower, RepairTower — 코어 UI 타워 탭의 칸 번호)
        public int slotIndex;
        // 대상 타워 종류 (UnlockTowerType, BuildTower)
        public int towerTypeIndex;
        // 고른 선택지 번호 0 ~ 2 (ChooseLevelUpOption)
        public int optionIndex;
    }
}
