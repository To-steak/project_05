namespace Project05.Component.Data
{
    /// <summary>
    /// 베이킹된 테이블 모음 (App.Data의 타입). 판 시작 시 World가 각 테이블 참조를 받는다. 읽기 전용으로 쓴다.
    /// </summary>
    public class GameData
    {
        // 코어 최대 체력
        public int coreMaxHP;
        // 코어 포위: 가장 안쪽 고리 반지름
        public float siegeFirstRingRadius;
        // 코어 포위: 고리 사이 간격
        public float siegeRingSpacing;
        // 코어 포위: 한 고리 위 자리 사이 간격 (고리별 자리 수 = 둘레 ÷ 이 값)
        public float siegeSlotSpacing;
        // 히어로 수치
        public HeroStats heroStats;
        // 적 종류별 수치 (인덱스 = 적 종류)
        public EnemyStats[] enemyStats;
        // 타워 종류별 수치 (인덱스 = 타워 종류)
        public TowerStats[] towerStats;
        // 웨이브 규칙
        public WaveRuleStats waveRule;
        // 난이도 증가 계수
        public DifficultyStats difficulty;
        // 레벨업 선택지 목록
        public LevelUpOptionStats[] levelUpOptions;
        // 레벨 진행
        public ProgressionStats progression;
    }
}
