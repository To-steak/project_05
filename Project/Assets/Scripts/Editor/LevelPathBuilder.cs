using System.Collections.Generic;
using Project05.Authoring.Scene;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project05.Editor
{
    /// <summary>
    /// 칠한 타일과 씬의 스폰·코어로 적 경로를 만든다. 에디터 전용.
    /// 길 타일(LevelTileSet.pathPrefabs)이 칠해진 칸만 따라 스폰마다 코어 칸까지 가장 짧은 길을 찾고(너비 우선 탐색),
    /// 방향이 바뀌는 칸과 코어 바로 앞 칸에 웨이포인트를 둬 SpawnPointAuthoring에 연결한다.
    /// 스폰 칸과 코어 칸은 길 타일이 아니어도 지나갈 수 있는 칸으로 본다.
    /// 생성물(경로, 클릭용 바닥 Collider)은 "Level (Generated)" 아래에 두고, 다시 생성하면 그 아래를 지우고 새로 만든다. 전체가 Undo 한 번이다.
    /// </summary>
    public static class LevelPathBuilder
    {
        // 상하좌우 이웃 칸 (+Z, +X, -Z, -X 순서)
        private static readonly Vector2Int[] NEIGHBOR_STEPS =
        {
            new Vector2Int(0, 1),
            new Vector2Int(1, 0),
            new Vector2Int(0, -1),
            new Vector2Int(-1, 0),
        };

        /// <summary>
        /// 지금 열린 씬에 경로와 클릭용 바닥 Collider를 만든다. 문제가 있으면 대화 상자로 알리고, 길이 끊긴 스폰은 건너뛴다.
        /// 생성 루트 밖에 다른 WaypointPathAuthoring이 있으면 지울지 묻는다 (SceneBaker가 모두 굽기 때문).
        /// </summary>
        /// <param name="tileSet">격자와 길 타일 설정</param>
        public static void Build(LevelTileSet tileSet)
        {
            Scene scene = SceneManager.GetActiveScene();
            GameObject paintRoot = FindRootObject(scene, LevelTilePainterWindow.PAINT_ROOT_NAME);
            CoreAuthoring[] cores = Object.FindObjectsByType<CoreAuthoring>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            SpawnPointAuthoring[] spawns = Object.FindObjectsByType<SpawnPointAuthoring>(FindObjectsInactive.Exclude, FindObjectsSortMode.InstanceID);

            if (paintRoot == null || paintRoot.transform.childCount == 0)
            {
                EditorUtility.DisplayDialog("경로 생성", "칠한 타일이 없다. 먼저 타일 모드로 바닥을 칠한다.", "확인");
                return;
            }
            if (cores.Length == 0)
            {
                EditorUtility.DisplayDialog("경로 생성", "씬에 CoreAuthoring이 없다. 코어 모드로 코어 칸을 정한다.", "확인");
                return;
            }
            if (spawns.Length == 0)
            {
                EditorUtility.DisplayDialog("경로 생성", "씬에 SpawnPointAuthoring이 없다. 스폰 모드로 스폰 지점을 놓는다.", "확인");
                return;
            }

            HashSet<Vector2Int> pathCells = CollectPathCells(tileSet, paintRoot.transform);
            if (pathCells.Count == 0)
            {
                EditorUtility.DisplayDialog("경로 생성", "길로 표시한 타일이 칠해진 칸이 없다. 칠하기 창에서 썸네일 아래 '길'을 체크한다.", "확인");
                return;
            }

            GameObject generatedRoot = FindRootObject(scene, GENERATED_ROOT_NAME);
            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("경로 생성");

            if (!RemovePathsOutside(generatedRoot))
            {
                return;
            }

            if (generatedRoot == null)
            {
                generatedRoot = new GameObject(GENERATED_ROOT_NAME);
                Undo.RegisterCreatedObjectUndo(generatedRoot, "경로 생성");
            }
            ClearGeneratedRoot(generatedRoot.transform);

            Vector2Int coreCell = LevelGrid.GetCell(tileSet, cores[0].transform.position);
            Transform pathsRoot = CreateChild(generatedRoot.transform, "Paths");
            List<string> failures = new List<string>();
            List<Vector2Int> route = new List<Vector2Int>();
            int builtCount = 0;
            for (int spawnIndex = 0; spawnIndex < spawns.Length; spawnIndex++)
            {
                SpawnPointAuthoring spawn = spawns[spawnIndex];
                Vector2Int spawnCell = LevelGrid.GetCell(tileSet, spawn.transform.position);
                if (!TryFindRoute(pathCells, spawnCell, coreCell, route))
                {
                    failures.Add("'" + spawn.name + "'에서 코어까지 길 타일이 이어지지 않는다.");
                    continue;
                }
                if (route.Count < 3)
                {
                    failures.Add("'" + spawn.name + "'이(가) 코어와 붙어 있다. 사이에 길이 한 칸 이상 있어야 한다.");
                    continue;
                }
                if (BuildPath(tileSet, spawn, route, pathsRoot, builtCount + 1))
                {
                    builtCount++;
                }
            }

            if (tileSet.createGroundCollider)
            {
                BuildGroundCollider(tileSet, paintRoot.transform, generatedRoot.transform);
            }

            Undo.CollapseUndoOperations(undoGroup);
            EditorSceneManager.MarkSceneDirty(scene);
            Selection.activeGameObject = generatedRoot;

            if (cores.Length > 1)
            {
                failures.Add("씬에 CoreAuthoring이 " + cores.Length + "개 있다. '" + cores[0].name + "'을(를) 기준으로 했다.");
            }
            string summary = "스폰 " + spawns.Length + "개 중 " + builtCount + "개의 경로를 만들었다. NavMesh를 다시 구워야 한다.";
            if (failures.Count > 0)
            {
                EditorUtility.DisplayDialog("경로 생성", summary + "\n\n" + string.Join("\n", failures), "확인");
            }
            Debug.Log("[LevelPathBuilder] " + summary, generatedRoot);
        }

        /// <summary>
        /// 길 타일이 칠해진 칸을 모은다. 칠한 타일의 원본 프리팹이 pathPrefabs에 있으면 길이다.
        /// </summary>
        private static HashSet<Vector2Int> CollectPathCells(LevelTileSet tileSet, Transform paintRoot)
        {
            HashSet<Vector2Int> pathCells = new HashSet<Vector2Int>();
            for (int childIndex = 0; childIndex < paintRoot.childCount; childIndex++)
            {
                Transform tile = paintRoot.GetChild(childIndex);
                GameObject source = PrefabUtility.GetCorrespondingObjectFromSource(tile.gameObject);
                if (source != null && IsPathPrefab(tileSet, source))
                {
                    pathCells.Add(LevelGrid.GetCell(tileSet, tile.position));
                }
            }
            return pathCells;
        }

        /// <summary>
        /// 프리팹이 길 타일 목록에 있는지.
        /// </summary>
        /// <param name="tileSet">격자와 길 타일 설정</param>
        /// <param name="prefab">확인할 프리팹</param>
        /// <returns>길 타일이면 true</returns>
        public static bool IsPathPrefab(LevelTileSet tileSet, GameObject prefab)
        {
            GameObject[] pathPrefabs = tileSet.pathPrefabs;
            if (pathPrefabs == null || prefab == null)
            {
                return false;
            }
            for (int pathIndex = 0; pathIndex < pathPrefabs.Length; pathIndex++)
            {
                if (pathPrefabs[pathIndex] == prefab)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 경로 하나를 만들고 스폰에 연결한다. 방향이 바뀌는 칸과 코어 바로 앞 칸이 웨이포인트다 (스폰 칸은 스폰 지점 자체라 넣지 않는다).
        /// </summary>
        private static bool BuildPath(LevelTileSet tileSet, SpawnPointAuthoring spawn, List<Vector2Int> route, Transform pathsRoot, int pathNumber)
        {
            Transform pathTransform = CreateChild(pathsRoot, "Path " + pathNumber + " (" + spawn.name + ")");
            WaypointPathAuthoring path = pathTransform.gameObject.AddComponent<WaypointPathAuthoring>();
            Vector3 pathHeight = Vector3.up * tileSet.pathSurfaceHeight;
            int waypointCount = 0;
            for (int routeIndex = 1; routeIndex < route.Count - 1; routeIndex++)
            {
                Vector2Int previous = route[routeIndex - 1];
                Vector2Int current = route[routeIndex];
                Vector2Int next = route[routeIndex + 1];
                bool isLastBeforeCore = routeIndex == route.Count - 2;
                if (!isLastBeforeCore && current - previous == next - current)
                {
                    continue;
                }

                waypointCount++;
                Transform waypoint = CreateChild(pathTransform, "Waypoint " + waypointCount);
                waypoint.position = LevelGrid.GetCellCenter(tileSet, current) + pathHeight;
            }

            SerializedObject serializedSpawn = new SerializedObject(spawn);
            SerializedProperty pathProperty = serializedSpawn.FindProperty(SPAWN_PATH_PROPERTY_NAME);
            if (pathProperty == null)
            {
                // SpawnPointAuthoring의 필드 이름이 바뀌면 여기서 알린다
                Debug.LogError("[LevelPathBuilder] SpawnPointAuthoring에 직렬화 필드 '" + SPAWN_PATH_PROPERTY_NAME + "'이(가) 없다. '" + spawn.name + "'에 경로를 연결하지 못했다. SPAWN_PATH_PROPERTY_NAME을 맞춘다.", spawn);
                return false;
            }
            pathProperty.objectReferenceValue = path;
            serializedSpawn.ApplyModifiedProperties();
            return true;
        }

        /// <summary>
        /// 칠한 타일 범위(칸 단위)를 덮는 얇은 BoxCollider 하나를 풀 바닥 높이에 만든다.
        /// </summary>
        private static void BuildGroundCollider(LevelTileSet tileSet, Transform paintRoot, Transform generatedRoot)
        {
            Vector2Int min = new Vector2Int(int.MaxValue, int.MaxValue);
            Vector2Int max = new Vector2Int(int.MinValue, int.MinValue);
            for (int childIndex = 0; childIndex < paintRoot.childCount; childIndex++)
            {
                Vector2Int cell = LevelGrid.GetCell(tileSet, paintRoot.GetChild(childIndex).position);
                min = Vector2Int.Min(min, cell);
                max = Vector2Int.Max(max, cell);
            }

            Vector3 minCenter = LevelGrid.GetCellCenter(tileSet, min);
            Vector3 maxCenter = LevelGrid.GetCellCenter(tileSet, max);
            Transform colliderTransform = CreateChild(generatedRoot, "Ground Collider");
            colliderTransform.position = new Vector3(
                (minCenter.x + maxCenter.x) * 0.5f,
                tileSet.origin.y + tileSet.grassSurfaceHeight - GROUND_COLLIDER_THICKNESS * 0.5f,
                (minCenter.z + maxCenter.z) * 0.5f);

            BoxCollider boxCollider = colliderTransform.gameObject.AddComponent<BoxCollider>();
            boxCollider.size = new Vector3(
                (max.x - min.x + 1) * tileSet.cellSize,
                GROUND_COLLIDER_THICKNESS,
                (max.y - min.y + 1) * tileSet.cellSize);
        }

        /// <summary>
        /// 생성 루트 아래를 지운다. 예전 버전이 이 아래에 만든 스폰·공터는 지우지 않고 "Level Markers"로 옮긴다.
        /// </summary>
        private static void ClearGeneratedRoot(Transform generatedRoot)
        {
            MoveMarkersOut<SpawnPointAuthoring>(generatedRoot, SPAWNS_GROUP_NAME);
            MoveMarkersOut<TowerSlotAuthoring>(generatedRoot, TOWER_SLOTS_GROUP_NAME);
            for (int childIndex = generatedRoot.childCount - 1; childIndex >= 0; childIndex--)
            {
                Undo.DestroyObjectImmediate(generatedRoot.GetChild(childIndex).gameObject);
            }
        }

        private static void MoveMarkersOut<T>(Transform generatedRoot, string groupName) where T : MonoBehaviour
        {
            T[] markers = generatedRoot.GetComponentsInChildren<T>(true);
            if (markers.Length == 0)
            {
                return;
            }

            Transform group = GetOrCreateMarkerGroup(groupName);
            for (int markerIndex = 0; markerIndex < markers.Length; markerIndex++)
            {
                Undo.SetTransformParent(markers[markerIndex].transform, group, "경로 생성");
            }
        }

        /// <summary>
        /// 생성 루트 밖의 WaypointPathAuthoring을 지울지 묻고 지운다. 취소하거나 지울 수 없으면 false.
        /// </summary>
        private static bool RemovePathsOutside(GameObject generatedRoot)
        {
            WaypointPathAuthoring[] paths = Object.FindObjectsByType<WaypointPathAuthoring>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            List<GameObject> targets = new List<GameObject>();
            List<string> names = new List<string>();
            for (int pathIndex = 0; pathIndex < paths.Length; pathIndex++)
            {
                GameObject target = paths[pathIndex].gameObject;
                if (generatedRoot != null && target.transform.IsChildOf(generatedRoot.transform))
                {
                    continue;
                }
                if (PrefabUtility.IsPartOfPrefabInstance(target) && !PrefabUtility.IsOutermostPrefabInstanceRoot(target))
                {
                    EditorUtility.DisplayDialog("경로 생성", "'" + target.name + "'은(는) 프리팹 안에 있어 지울 수 없다. 프리팹에서 직접 지운 뒤 다시 시도한다.", "확인");
                    return false;
                }
                targets.Add(target);
                names.Add(target.name);
            }
            if (targets.Count == 0)
            {
                return true;
            }

            bool shouldRemove = EditorUtility.DisplayDialog(
                "경로 생성",
                "씬에 직접 만든 경로(WaypointPathAuthoring)가 " + targets.Count + "개 있다. 남겨 두면 쓰이지 않는 경로가 함께 구워진다. 지우고 새로 만들까?\n\n" + string.Join("\n", names),
                "지우고 생성",
                "취소");
            if (!shouldRemove)
            {
                return false;
            }

            for (int targetIndex = 0; targetIndex < targets.Count; targetIndex++)
            {
                if (targets[targetIndex] != null)
                {
                    Undo.DestroyObjectImmediate(targets[targetIndex]);
                }
            }
            return true;
        }

        /// <summary>
        /// "Level Markers" 아래의 묶음(스폰, 공터)을 찾거나 만든다. 칠하기 창이 스폰·공터를 놓을 때도 쓴다.
        /// </summary>
        /// <param name="groupName">묶음 이름</param>
        /// <returns>묶음 Transform</returns>
        public static Transform GetOrCreateMarkerGroup(string groupName)
        {
            Scene scene = SceneManager.GetActiveScene();
            GameObject markersRoot = FindRootObject(scene, MARKERS_ROOT_NAME);
            if (markersRoot == null)
            {
                markersRoot = new GameObject(MARKERS_ROOT_NAME);
                Undo.RegisterCreatedObjectUndo(markersRoot, "레벨 표시");
            }

            Transform group = markersRoot.transform.Find(groupName);
            if (group == null)
            {
                group = CreateChild(markersRoot.transform, groupName);
            }
            return group;
        }

        /// <summary>
        /// 길 칸만 밟는 가장 짧은 경로 (너비 우선 탐색). 시작 칸과 목표 칸은 길이 아니어도 된다.
        /// </summary>
        /// <param name="route">시작부터 목표까지의 칸. 먼저 비운다</param>
        private static bool TryFindRoute(HashSet<Vector2Int> pathCells, Vector2Int start, Vector2Int goal, List<Vector2Int> route)
        {
            route.Clear();
            Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
            Queue<Vector2Int> frontier = new Queue<Vector2Int>();
            frontier.Enqueue(start);
            cameFrom[start] = start;

            while (frontier.Count > 0)
            {
                Vector2Int current = frontier.Dequeue();
                if (current == goal)
                {
                    for (Vector2Int step = goal; step != start; step = cameFrom[step])
                    {
                        route.Add(step);
                    }
                    route.Add(start);
                    route.Reverse();
                    return true;
                }

                for (int stepIndex = 0; stepIndex < NEIGHBOR_STEPS.Length; stepIndex++)
                {
                    Vector2Int next = current + NEIGHBOR_STEPS[stepIndex];
                    if (cameFrom.ContainsKey(next) || (next != goal && !pathCells.Contains(next)))
                    {
                        continue;
                    }
                    cameFrom[next] = current;
                    frontier.Enqueue(next);
                }
            }
            return false;
        }

        private static GameObject FindRootObject(Scene scene, string objectName)
        {
            GameObject[] rootObjects = scene.GetRootGameObjects();
            for (int rootIndex = 0; rootIndex < rootObjects.Length; rootIndex++)
            {
                if (rootObjects[rootIndex].name == objectName)
                {
                    return rootObjects[rootIndex];
                }
            }
            return null;
        }

        private static Transform CreateChild(Transform parent, string childName)
        {
            GameObject child = new GameObject(childName);
            Undo.RegisterCreatedObjectUndo(child, "레벨 생성");
            child.transform.SetParent(parent, false);
            return child.transform;
        }

        // 경로·바닥 Collider를 모아 두는 씬 루트 이름 (경로 생성마다 다시 만든다)
        public const string GENERATED_ROOT_NAME = "Level (Generated)";
        // 칠하기 창이 놓은 스폰·공터를 모아 두는 씬 루트 이름 (경로 생성이 지우지 않는다)
        public const string MARKERS_ROOT_NAME = "Level Markers";
        // 스폰 묶음 이름
        public const string SPAWNS_GROUP_NAME = "Spawns";
        // 공터 묶음 이름
        public const string TOWER_SLOTS_GROUP_NAME = "Tower Slots";

        private const float GROUND_COLLIDER_THICKNESS = 0.1f;
        // SpawnPointAuthoring의 경로 직렬화 필드 이름
        private const string SPAWN_PATH_PROPERTY_NAME = "_path";
    }
}
