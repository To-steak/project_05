using UnityEngine;

namespace Project05.Authoring.Definition
{
    /// <summary>
    /// 모든 Definition을 가진 루트. Resources/GameDefinition에 두며, 게임 시작 시 DefinitionBaker가 읽는다.
    /// 지금은 히어로만 있다. 나머지 Definition은 feat/baking에서 필드로 추가한다.
    /// </summary>
    [CreateAssetMenu(fileName = "GameDefinition", menuName = "Project05/Game Definition")]
    public class GameDefinition : ScriptableObject
    {
        // 히어로 정의
        public HeroDefinition heroDefinition;
    }
}
