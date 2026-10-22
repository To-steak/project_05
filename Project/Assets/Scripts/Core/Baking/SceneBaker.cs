using Project05.Authoring.Scene;
using Project05.Component;
using UnityEngine;

namespace Project05.Core.Baking
{
    /// <summary>
    /// 판 시작 시, 씬의 ~Authoring 컴포넌트를 World로 베이킹한다.
    /// 지금은 히어로 시작 위치만 베이킹한다. 웨이포인트·스폰·코어·공터는 feat/baking에서 추가한다.
    /// </summary>
    public static class SceneBaker
    {
        /// <summary>
        /// 활성 씬의 Authoring 값을 World에 쓴다. SessionSystem.Initialize 이후, SessionSystem.Begin 이전에 호출한다.
        /// </summary>
        /// <param name="world">초기화된 월드</param>
        public static void Bake(World world)
        {
            HeroSpawnAuthoring heroSpawn = Object.FindAnyObjectByType<HeroSpawnAuthoring>();
            if (heroSpawn == null)
            {
                Debug.LogWarning("[SceneBaker] 씬에 HeroSpawnAuthoring이 없어 히어로를 원점에 둡니다.");
                world.heroSpawnPosition = Vector3.zero;
            }
            else
            {
                world.heroSpawnPosition = heroSpawn.transform.position;
            }
        }
    }
}
