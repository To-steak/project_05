using UnityEngine;

namespace Project05.Authoring.Definition
{
    /// <summary>
    /// 웨이브 규칙. 게임 시작 시 DefinitionBaker가 WaveRuleStats로 베이킹한다.
    /// 웨이브 적 수는 임시 규칙 max = x × enemyCountPerWaveNumber (DESIGN 3.4.1). 적 구성 생성 규칙은 미정.
    /// </summary>
    [CreateAssetMenu(fileName = "WaveRuleDefinition", menuName = "Project05/Definition/Wave Rule")]
    public class WaveRuleDefinition : ScriptableObject
    {
        // 웨이브 적 수 계수. 웨이브 적 수 max = x × 이 값 (임시 규칙: 10)
        [Min(1)] public int enemyCountPerWaveNumber = 10;
        // 적 스폰 간격 (틱)
        [Min(1)] public int spawnIntervalTicks = 1;
        // 웨이브 사이 준비 시간 (틱)
        [Min(0)] public int preparationTicks;
        // 보스 웨이브 번호이자 스테이지당 웨이브 수 (10)
        [Min(1)] public int bossWaveNumber = 10;
    }
}
