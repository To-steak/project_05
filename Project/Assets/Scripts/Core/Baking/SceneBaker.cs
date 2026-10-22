using System.Collections.Generic;
using Project05.Authoring.Scene;
using Project05.Component;
using Project05.Component.Entity;
using Project05.Component.Path;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project05.Core.Baking
{
    /// <summary>
    /// 판 시작 시 씬의 Authoring 컴포넌트를 World의 배치 데이터로 베이킹한다.
    /// 경로·스폰 지점·공터의 인덱스는 하이어라키 순서(루트 순서 → 깊이 우선)로 정해져 매 판 같다.
    /// 비활성 오브젝트는 베이킹하지 않는다. 판 시작 시 1회만 실행되므로 할당은 신경 쓰지 않는다.
    /// </summary>
    public static class SceneBaker
    {
        /// <summary>
        /// 씬의 경로, 스폰 지점, 공터, 코어 위치, 히어로 시작 위치를 World에 채운다.
        /// SessionSystem.Initialize보다 먼저 호출한다 (타워 저장소 용량과 히어로 시작 위치가 이 값을 쓴다).
        /// </summary>
        /// <param name="world">new World()로 막 만든 월드</param>
        /// <param name="scene">베이킹할 씬 (판이 시작된 활성 씬)</param>
        public static void Bake(World world, Scene scene)
        {
            List<WaypointPathAuthoring> pathAuthorings = Collect<WaypointPathAuthoring>(scene);
            world.waypointPaths = BakeWaypointPaths(pathAuthorings);
            world.spawnPoints = BakeSpawnPoints(Collect<SpawnPointAuthoring>(scene), pathAuthorings);
            world.towerSlots = BakeTowerSlots(Collect<TowerSlotAuthoring>(scene));
            world.core.position = BakeSinglePosition(Collect<CoreAuthoring>(scene), "CoreAuthoring");
            world.heroSpawnPosition = BakeSinglePosition(Collect<HeroSpawnAuthoring>(scene), "HeroSpawnAuthoring");
        }

        private static WaypointPath[] BakeWaypointPaths(List<WaypointPathAuthoring> pathAuthorings)
        {
            WaypointPath[] waypointPaths = new WaypointPath[pathAuthorings.Count];
            for (int pathIndex = 0; pathIndex < pathAuthorings.Count; pathIndex++)
            {
                WaypointPathAuthoring pathAuthoring = pathAuthorings[pathIndex];
                int waypointCount = pathAuthoring.WaypointCount;
                if (waypointCount == 0)
                {
                    Debug.LogWarning("[SceneBaker] 경로 " + pathAuthoring.name + "에 웨이포인트(자식)가 없다.", pathAuthoring);
                }

                ref WaypointPath waypointPath = ref waypointPaths[pathIndex];
                waypointPath.waypoints = new Vector3[waypointCount];
                for (int waypointIndex = 0; waypointIndex < waypointCount; waypointIndex++)
                {
                    waypointPath.waypoints[waypointIndex] = pathAuthoring.GetWaypointPosition(waypointIndex);
                }
            }
            return waypointPaths;
        }

        private static SpawnPoint[] BakeSpawnPoints(List<SpawnPointAuthoring> spawnAuthorings, List<WaypointPathAuthoring> pathAuthorings)
        {
            List<SpawnPoint> spawnPoints = new List<SpawnPoint>(spawnAuthorings.Count);
            for (int spawnIndex = 0; spawnIndex < spawnAuthorings.Count; spawnIndex++)
            {
                SpawnPointAuthoring spawnAuthoring = spawnAuthorings[spawnIndex];
                // 비활성 경로나 다른 씬의 경로를 가리키면 목록에 없으므로 -1이 된다
                int pathIndex = spawnAuthoring.Path == null ? -1 : pathAuthorings.IndexOf(spawnAuthoring.Path);
                if (pathIndex < 0)
                {
                    Debug.LogWarning("[SceneBaker] 스폰 지점 " + spawnAuthoring.name + "에 연결된 경로가 없어 제외한다.", spawnAuthoring);
                    continue;
                }

                SpawnPoint spawnPoint = new SpawnPoint();
                spawnPoint.position = spawnAuthoring.transform.position;
                spawnPoint.pathIndex = pathIndex;
                spawnPoints.Add(spawnPoint);
            }
            return spawnPoints.ToArray();
        }

        private static TowerSlot[] BakeTowerSlots(List<TowerSlotAuthoring> slotAuthorings)
        {
            TowerSlot[] towerSlots = new TowerSlot[slotAuthorings.Count];
            for (int slotIndex = 0; slotIndex < slotAuthorings.Count; slotIndex++)
            {
                ref TowerSlot towerSlot = ref towerSlots[slotIndex];
                towerSlot.position = slotAuthorings[slotIndex].transform.position;
                towerSlot.towerID = default(EntityID);
                towerSlot.isBuildPending = false;
            }
            return towerSlots;
        }

        private static Vector3 BakeSinglePosition<T>(List<T> authorings, string authoringName) where T : MonoBehaviour
        {
            if (authorings.Count == 0)
            {
                Debug.LogWarning("[SceneBaker] 씬에 " + authoringName + "가 없다. 원점을 쓴다.");
                return Vector3.zero;
            }
            if (authorings.Count > 1)
            {
                Debug.LogWarning("[SceneBaker] 씬에 " + authoringName + "가 " + authorings.Count + "개 있다. 첫 번째만 쓴다.", authorings[0]);
            }
            return authorings[0].transform.position;
        }

        /// <summary>
        /// 씬의 활성 오브젝트에서 T를 하이어라키 순서로 모은다.
        /// </summary>
        private static List<T> Collect<T>(Scene scene) where T : MonoBehaviour
        {
            List<T> collected = new List<T>();
            List<T> buffer = new List<T>();
            GameObject[] rootObjects = scene.GetRootGameObjects();
            for (int rootIndex = 0; rootIndex < rootObjects.Length; rootIndex++)
            {
                rootObjects[rootIndex].GetComponentsInChildren<T>(false, buffer);
                collected.AddRange(buffer);
            }
            return collected;
        }
    }
}
