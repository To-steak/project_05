using UnityEngine;

namespace Project05.Authoring.Definition
{
    /// <summary>
    /// 모든 Definition을 가진 루트. Resources 폴더의 RESOURCE_PATH에 하나만 두고,
    /// 게임 시작 시 Bootstrap이 불러와 DefinitionBaker로 App.Data에 베이킹한다.
    /// 타워·레벨업 선택지 정의는 해당 기능 브랜치에서 추가한다.
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
        // 코어 최대 체력
        [Min(1)] public int coreMaxHP = 1;

        // Resources.Load 경로 (Assets/Resources/GameDefinition.asset)
        public const string RESOURCE_PATH = "GameDefinition";
    }
}
