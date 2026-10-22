using Project05.Authoring.Definition;
using Project05.Component.Data;
using UnityEngine;

namespace Project05.Core.Baking
{
    /// <summary>
    /// 게임 시작 시 1회, ~Definition을 GameData struct 테이블로 베이킹한다.
    /// 지금은 히어로만 베이킹한다. 나머지 테이블은 feat/baking에서 추가한다.
    /// </summary>
    public static class DefinitionBaker
    {
        private const string GAME_DEFINITION_RESOURCE_PATH = "GameDefinition";

        /// <summary>
        /// Resources의 GameDefinition을 읽어 GameData를 만든다. 정의가 없으면 빈 GameData를 돌려주고 에러를 남긴다.
        /// </summary>
        /// <returns>베이킹된 테이블 모음 (null이 아님)</returns>
        public static GameData Bake()
        {
            GameData gameData = new GameData();

            GameDefinition gameDefinition = Resources.Load<GameDefinition>(GAME_DEFINITION_RESOURCE_PATH);
            if (gameDefinition == null)
            {
                Debug.LogError("[DefinitionBaker] Resources/" + GAME_DEFINITION_RESOURCE_PATH + " 에셋이 없습니다.");
                return gameData;
            }

            if (gameDefinition.heroDefinition == null)
            {
                Debug.LogError("[DefinitionBaker] GameDefinition에 HeroDefinition이 연결되지 않았습니다.");
            }
            else
            {
                gameData.heroStats = BakeHero(gameDefinition.heroDefinition);
            }

            return gameData;
        }

        private static HeroStats BakeHero(HeroDefinition heroDefinition)
        {
            HeroStats heroStats = new HeroStats();
            heroStats.maxHP = heroDefinition.maxHP;
            heroStats.lifeCount = heroDefinition.lifeCount;
            heroStats.moveSpeed = heroDefinition.moveSpeed;
            heroStats.attackDamage = heroDefinition.attackDamage;
            heroStats.attackIntervalTicks = heroDefinition.attackIntervalTicks;
            heroStats.meleeRange = heroDefinition.meleeRange;
            heroStats.rangedRange = heroDefinition.rangedRange;
            heroStats.contactRadius = heroDefinition.contactRadius;
            heroStats.invincibilityTicks = heroDefinition.invincibilityTicks;
            heroStats.respawnTicks = heroDefinition.respawnTicks;
            heroStats.pickupRadius = heroDefinition.pickupRadius;
            heroStats.interactionRadius = heroDefinition.interactionRadius;
            return heroStats;
        }
    }
}
