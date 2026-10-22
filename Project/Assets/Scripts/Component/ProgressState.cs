namespace Project05.Component
{
    /// <summary>
    /// 성장·경제 상태.
    /// </summary>
    public struct ProgressState
    {
        // 보유 골드
        public int gold;
        // 현재 레벨에서 모은 XP
        public int currentXP;
        // 히어로 레벨 (1부터)
        public int level;
        // 선택을 기다리는 레벨업 수
        public int pendingLevelUpCount;
        // 현재 제시된 선택지 (LevelUpOptionStats 테이블 인덱스, 길이 OFFERED_OPTION_COUNT)
        public int[] offeredOptionIndices;
        // 드롭 골드 획득 퍼센트 (100 = 1배)
        public int goldGainPercent;
        // 드롭 XP 획득 퍼센트 (100 = 1배)
        public int xpGainPercent;
        // 점수 (계산 규칙 미정)
        public int score;

        // 한 번에 제시하는 레벨업 선택지 수
        public const int OFFERED_OPTION_COUNT = 3;
    }
}
