using UnityEngine;

namespace Project05.Authoring.Definition
{
    /// <summary>
    /// 모든 Definition을 가진 루트. Resources 폴더의 RESOURCE_PATH에 하나만 두고,
    /// 게임 시작 시 Bootstrap이 불러와 DefinitionBaker로 App.Data에 베이킹한다.
    /// </summary>
    [CreateAssetMenu(fileName = "GameDefinition", menuName = "Project05/Definition/Game")]
    public class GameDefinition : ScriptableObject
    {
        // 히어로 정의
        public HeroDefinition hero;
        // 적 종류별 정의 (인덱스 = 적 종류, 보스 포함)
        public EnemyDefinition[] enemies = new EnemyDefinition[0];
        // 웨이브 규칙
        public WaveRuleDefinition waveRule;
        // 난이도 증가 계수
        public DifficultyDefinition difficulty;
        // 레벨 진행
        public ProgressionDefinition progression;
        // 타워 종류별 정의 (인덱스 = 타워 종류)
        public TowerDefinition[] towers = new TowerDefinition[0];
        // 레벨업 선택지 목록 (레벨업마다 이 중 3개를 중복 없이 뽑아 제시)
        public LevelUpOptionDefinition[] levelUpOptions = new LevelUpOptionDefinition[0];
        // 배경음·효과음 클립과 믹스 수치 (오디오 테이블로 베이킹, 없으면 소리가 나지 않는다)
        public AudioDefinition audio;
        // 골드 드롭 View 프리팹 (View 테이블로 베이킹, 없으면 보이지 않는다)
        public GameObject goldDropPrefab;
        // XP 드롭 View 프리팹 (View 테이블로 베이킹, 없으면 보이지 않는다)
        public GameObject xpDropPrefab;
        // 코어 최대 체력
        [Min(1)] public int coreMaxHP = 1;
        // 코어 포위: 가장 안쪽 고리 반지름 (코어 중심에서)
        [Min(0f)] public float siegeFirstRingRadius = 2.5f;
        // 코어 포위: 고리 사이 간격
        [Min(0.1f)] public float siegeRingSpacing = 1.2f;
        // 코어 포위: 한 고리 위 자리 사이 간격 (고리별 자리 수 = 둘레 ÷ 이 값)
        [Min(0.1f)] public float siegeSlotSpacing = 1.2f;

        // Resources.Load 경로 (Assets/Resources/GameDefinition.asset)
        public const string RESOURCE_PATH = "GameDefinition";
    }
}
