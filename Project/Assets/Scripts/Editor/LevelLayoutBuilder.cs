using System.Collections.Generic;
using Project05.Authoring.Scene;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project05.Editor
{
    /// <summary>
    /// LevelLayout 격자로 씬에 레벨을 만든다. 에디터 전용.
    /// 길 칸은 이웃한 길 칸(상하좌우)을 보고 직선·굽은 길·갈림길·교차로·막다른 길 중 하나를 골라 방향을 맞춘다.
    /// 스폰마다 코어까지 가장 짧은 길을 찾아 꺾이는 칸에 웨이포인트를 두고, 공터·코어 위치와 클릭용 바닥 Collider도 함께 만든다.
    /// 생성물은 모두 "Level (Generated)" 아래에 두고, 다시 생성하면 그 아래를 지우고 새로 만든다. 전체가 Undo 한 번으로 되돌아간다.
    /// </summary>
    public static class LevelLayoutBuilder
    {
        // 상하좌우 이웃 순서: 북, 동, 남, 서 (열린 방향 비트도 같은 순서)
        private static readonly int[] ROW_STEPS = { -1, 0, 1, 0 };
        private static readonly int[] COLUMN_STEPS = { 0, 1, 0, -1 };

        /// <summary>
        /// 격자 텍스트를 칸 배열로 바꾼다. 공백은 무시하고, 줄 길이가 다르면 짧은 줄을 빈칸으로 채운다.
        /// 모르는 기호는 빈칸으로 바꾸고 오류를 남긴다.
        /// </summary>
        /// <param name="grid">격자 텍스트</param>
        /// <param name="errors">오류를 더할 목록</param>
        /// <param name="warnings">경고를 더할 목록</param>
        /// <returns>[행, 열] 칸 배열. 비어 있으면 null</returns>
        public static char[,] ParseGrid(string grid, List<string> errors, List<string> warnings)
        {
            List<string> rows = new List<string>();
            int columnCount = 0;
            string[] lines = (grid ?? string.Empty).Split('\n');
            for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                string row = lines[lineIndex].Replace("\r", string.Empty).Replace(" ", string.Empty).Replace("\t", string.Empty);
                if (row.Length == 0)
                {
                    continue;
                }
                rows.Add(row);
                columnCount = Mathf.Max(columnCount, row.Length);
            }

            if (rows.Count == 0)
            {
                errors.Add("격자가 비어 있다.");
                return null;
            }

            char[,] cells = new char[rows.Count, columnCount];
            bool hasRaggedRow = false;
            for (int row = 0; row < rows.Count; row++)
            {
                string rowText = rows[row];
                hasRaggedRow = hasRaggedRow || rowText.Length != columnCount;
                for (int column = 0; column < columnCount; column++)
                {
                    char symbol = column < rowText.Length ? rowText[column] : EMPTY_SYMBOL;
                    if (!IsKnownSymbol(symbol))
                    {
                        errors.Add("모르는 기호 '" + symbol + "' (" + (row + 1) + "행 " + (column + 1) + "열)");
                        symbol = EMPTY_SYMBOL;
                    }
                    cells[row, column] = symbol;
                }
            }

            if (hasRaggedRow)
            {
                warnings.Add("줄 길이가 서로 다르다. 짧은 줄의 끝은 빈칸으로 채운다.");
            }
            return cells;
        }

        /// <summary>
        /// 레벨을 만들 수 있는지 검사한다. 코어는 하나여야 하고, 모든 스폰에서 코어까지 길이 이어져야 한다.
        /// </summary>
        /// <param name="layout">검사할 레이아웃</param>
        /// <param name="errors">생성을 막는 오류 (먼저 비운다)</param>
        /// <param name="warnings">생성은 되지만 확인할 것 (먼저 비운다)</param>
        /// <returns>오류가 없으면 true</returns>
        public static bool Validate(LevelLayout layout, List<string> errors, List<string> warnings)
        {
            errors.Clear();
            warnings.Clear();
            char[,] cells = ParseGrid(layout.grid, errors, warnings);
            if (cells == null)
            {
                return false;
            }

            List<Vector2Int> cores = FindSymbols(cells, CORE_SYMBOL);
            List<Vector2Int> spawns = FindSymbols(cells, SPAWN_SYMBOL);
            if (cores.Count != 1)
            {
                errors.Add("코어(C)는 정확히 하나여야 한다. 지금 " + cores.Count + "개.");
            }
            if (spawns.Count == 0)
            {
                warnings.Add("스폰(S)이 없다. 적이 나오지 않는다.");
            }
            if (FindSymbols(cells, SLOT_SYMBOL).Count == 0)
            {
                warnings.Add("타워 공터(T)가 없다.");
            }

            if (cores.Count == 1)
            {
                List<Vector2Int> route = new List<Vector2Int>();
                for (int spawnIndex = 0; spawnIndex < spawns.Count; spawnIndex++)
                {
                    Vector2Int spawn = spawns[spawnIndex];
                    if (!TryFindRoute(cells, spawn, cores[0], route))
                    {
                        errors.Add("스폰 " + (spawnIndex + 1) + " (" + (spawn.y + 1) + "행 " + (spawn.x + 1) + "열)에서 코어까지 길(#)이 이어지지 않는다.");
                    }
                    else if (route.Count < 3)
                    {
                        errors.Add("스폰 " + (spawnIndex + 1) + "이 코어와 붙어 있다. 사이에 길이 한 칸 이상 있어야 웨이포인트를 둘 수 있다.");
                    }
                }
            }

            if (layout.generateTiles)
            {
                AddMissingPrefabWarnings(layout, cells, warnings);
            }
            return errors.Count == 0;
        }

        /// <summary>
        /// 레이아웃으로 지금 열린 씬에 레벨을 만든다. 오류가 있으면 대화 상자를 띄우고 멈춘다.
        /// "Level (Generated)" 밖에 스폰·경로·공터 Authoring이 있으면 지울지 묻고, 취소하면 아무것도 바꾸지 않는다.
        /// </summary>
        /// <param name="layout">만들 레이아웃</param>
        public static void Build(LevelLayout layout)
        {
            List<string> errors = new List<string>();
            List<string> warnings = new List<string>();
            if (!Validate(layout, errors, warnings))
            {
                EditorUtility.DisplayDialog("레벨 생성", "격자에 오류가 있다.\n\n" + string.Join("\n", errors), "확인");
                return;
            }

            // 검사를 통과했으니 다시 해석할 때 나오는 경고는 이미 warnings에 있다
            char[,] cells = ParseGrid(layout.grid, new List<string>(), new List<string>());
            Scene scene = SceneManager.GetActiveScene();
            GameObject root = FindRoot(scene);

            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("레벨 생성");

            if (!RemoveAuthoringsOutside(root))
            {
                return;
            }

            if (root == null)
            {
                root = new GameObject(ROOT_NAME);
                SceneManager.MoveGameObjectToScene(root, scene);
                Undo.RegisterCreatedObjectUndo(root, "레벨 생성");
            }
            ClearChildren(root.transform);

            int tileCount = layout.generateTiles ? BuildTiles(layout, cells, root.transform) : 0;
            if (layout.createGroundCollider)
            {
                BuildGroundCollider(layout, cells, root.transform);
            }
            int spawnCount = BuildPaths(layout, cells, root.transform);
            int slotCount = BuildTowerSlots(layout, cells, root.transform);
            bool hasMovedCore = MoveCore(layout, cells);

            Undo.CollapseUndoOperations(undoGroup);
            EditorSceneManager.MarkSceneDirty(scene);
            Selection.activeGameObject = root;

            Debug.Log("[LevelLayoutBuilder] 타일 " + tileCount + "개, 스폰·경로 " + spawnCount + "개, 공터 " + slotCount + "개를 만들었다."
                + (hasMovedCore ? " 코어를 옮겼다." : " 씬에 CoreAuthoring이 없어 코어는 옮기지 않았다.")
                + " NavMesh를 다시 구워야 한다.", root);
            for (int warningIndex = 0; warningIndex < warnings.Count; warningIndex++)
            {
                Debug.LogWarning("[LevelLayoutBuilder] " + warnings[warningIndex], layout);
            }
        }

        /// <summary>
        /// 칸 기호가 길(적과 히어로가 다니는 흙 길)인지. 스폰과 코어 칸도 길로 이어진다.
        /// </summary>
        /// <param name="symbol">칸 기호</param>
        /// <returns>길이면 true</returns>
        public static bool IsPathSymbol(char symbol)
        {
            return symbol == PATH_SYMBOL || symbol == SPAWN_SYMBOL || symbol == CORE_SYMBOL;
        }

        /// <summary>
        /// 칸의 타일 종류와 회전(도)을 정한다. 길 칸은 열린 방향에 맞는 종류를 기준 방향에서 90도씩 돌려 맞춘다.
        /// 빈칸이면 false.
        /// </summary>
        /// <param name="cells">[행, 열] 칸 배열</param>
        /// <param name="row">행</param>
        /// <param name="column">열</param>
        /// <param name="tileKind">타일 종류</param>
        /// <param name="quarterTurns">기준 방향에서 시계 방향으로 돌릴 90도 횟수 (0 ~ 3)</param>
        /// <returns>타일을 놓을 칸이면 true</returns>
        public static bool TryGetTile(char[,] cells, int row, int column, out LevelTileKind tileKind, out int quarterTurns)
        {
            tileKind = LevelTileKind.Grass;
            quarterTurns = 0;
            char symbol = cells[row, column];
            if (symbol == EMPTY_SYMBOL)
            {
                return false;
            }
            if (!IsPathSymbol(symbol))
            {
                return true;
            }

            int openMask = GetOpenMask(cells, row, column);
            int canonicalMask;
            switch (CountBits(openMask))
            {
                case 0:
                    // 이어진 길이 없는 외딴 길 칸은 직선으로 둔다
                    tileKind = LevelTileKind.Straight;
                    return true;
                case 1:
                    tileKind = LevelTileKind.End;
                    canonicalMask = NORTH_MASK;
                    break;
                case 2:
                    bool isStraight = openMask == (NORTH_MASK | SOUTH_MASK) || openMask == (EAST_MASK | WEST_MASK);
                    tileKind = isStraight ? LevelTileKind.Straight : LevelTileKind.Bend;
                    canonicalMask = isStraight ? NORTH_MASK | SOUTH_MASK : NORTH_MASK | EAST_MASK;
                    break;
                case 3:
                    tileKind = LevelTileKind.Split;
                    canonicalMask = NORTH_MASK | EAST_MASK | SOUTH_MASK;
                    break;
                default:
                    tileKind = LevelTileKind.Cross;
                    return true;
            }

            int rotatedMask = canonicalMask;
            for (int turns = 0; turns < 4; turns++)
            {
                if (rotatedMask == openMask)
                {
                    quarterTurns = turns;
                    return true;
                }
                rotatedMask = RotateMaskClockwise(rotatedMask);
            }
            return true;
        }

        /// <summary>
        /// 칸 중심의 월드 위치 (높이는 origin.y).
        /// </summary>
        private static Vector3 GetCellCenter(LevelLayout layout, int row, int column)
        {
            return layout.origin + new Vector3(column * layout.cellSize, 0f, -row * layout.cellSize);
        }

        private static int BuildTiles(LevelLayout layout, char[,] cells, Transform root)
        {
            Transform tilesRoot = CreateChild(root, "Tiles");
            int tileCount = 0;
            for (int row = 0; row < cells.GetLength(0); row++)
            {
                for (int column = 0; column < cells.GetLength(1); column++)
                {
                    LevelTileKind tileKind;
                    int quarterTurns;
                    if (!TryGetTile(cells, row, column, out tileKind, out quarterTurns))
                    {
                        continue;
                    }

                    float yawOffset;
                    GameObject prefab = GetTilePrefab(layout, tileKind, out yawOffset);
                    if (prefab == null)
                    {
                        continue;
                    }

                    GameObject tile = (GameObject)PrefabUtility.InstantiatePrefab(prefab, tilesRoot);
                    Undo.RegisterCreatedObjectUndo(tile, "레벨 생성");
                    tile.name = tileKind + " " + (row + 1) + "," + (column + 1);
                    tile.transform.SetPositionAndRotation(
                        GetCellCenter(layout, row, column),
                        Quaternion.Euler(0f, quarterTurns * 90f + yawOffset, 0f) * prefab.transform.rotation);
                    tile.transform.localScale = prefab.transform.localScale * layout.tileScale;
                    if (layout.markTilesStatic)
                    {
                        MarkBatchingStatic(tile.transform);
                    }
                    tileCount++;
                }
            }
            return tileCount;
        }

        /// <summary>
        /// 격자 전체를 덮는 얇은 BoxCollider 하나를 풀 바닥 높이에 만든다. 클릭 레이캐스트가 타일 모양과 상관없이 바닥에 닿게 한다.
        /// </summary>
        private static void BuildGroundCollider(LevelLayout layout, char[,] cells, Transform root)
        {
            int rowCount = cells.GetLength(0);
            int columnCount = cells.GetLength(1);
            Transform colliderTransform = CreateChild(root, "Ground Collider");
            Vector3 center = layout.origin + new Vector3(
                (columnCount - 1) * layout.cellSize * 0.5f,
                layout.grassSurfaceHeight - GROUND_COLLIDER_THICKNESS * 0.5f,
                -(rowCount - 1) * layout.cellSize * 0.5f);
            colliderTransform.position = center;

            BoxCollider boxCollider = colliderTransform.gameObject.AddComponent<BoxCollider>();
            boxCollider.size = new Vector3(columnCount * layout.cellSize, GROUND_COLLIDER_THICKNESS, rowCount * layout.cellSize);
        }

        /// <summary>
        /// 스폰마다 스폰 지점과 경로를 만든다. 경로는 코어까지 가장 짧은 길에서 방향이 바뀌는 칸과, 코어 바로 앞 칸에 웨이포인트를 둔다.
        /// </summary>
        private static int BuildPaths(LevelLayout layout, char[,] cells, Transform root)
        {
            List<Vector2Int> spawns = FindSymbols(cells, SPAWN_SYMBOL);
            Vector2Int core = FindSymbols(cells, CORE_SYMBOL)[0];
            Transform pathsRoot = CreateChild(root, "Paths");
            Vector3 pathHeight = Vector3.up * layout.pathSurfaceHeight;
            List<Vector2Int> route = new List<Vector2Int>();

            for (int spawnIndex = 0; spawnIndex < spawns.Count; spawnIndex++)
            {
                Vector2Int spawn = spawns[spawnIndex];
                TryFindRoute(cells, spawn, core, route);

                Transform pathTransform = CreateChild(pathsRoot, "Path " + (spawnIndex + 1));
                WaypointPathAuthoring path = pathTransform.gameObject.AddComponent<WaypointPathAuthoring>();
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
                    waypoint.position = GetCellCenter(layout, current.y, current.x) + pathHeight;
                }

                Transform spawnTransform = CreateChild(pathsRoot, "Spawn " + (spawnIndex + 1));
                spawnTransform.position = GetCellCenter(layout, spawn.y, spawn.x) + pathHeight;
                SpawnPointAuthoring spawnPoint = spawnTransform.gameObject.AddComponent<SpawnPointAuthoring>();
                SerializedObject serializedSpawnPoint = new SerializedObject(spawnPoint);
                SerializedProperty pathProperty = serializedSpawnPoint.FindProperty(SPAWN_PATH_PROPERTY_NAME);
                if (pathProperty == null)
                {
                    // SpawnPointAuthoring의 필드 이름이 바뀌면 여기서 알린다 (경로 없이 스폰만 만들어진다)
                    Debug.LogError("[LevelLayoutBuilder] SpawnPointAuthoring에 직렬화 필드 '" + SPAWN_PATH_PROPERTY_NAME + "'이(가) 없다. 스폰 " + (spawnIndex + 1) + "에 경로를 연결하지 못했다. SPAWN_PATH_PROPERTY_NAME을 맞춘다.", spawnPoint);
                    continue;
                }
                pathProperty.objectReferenceValue = path;
                serializedSpawnPoint.ApplyModifiedPropertiesWithoutUndo();
            }
            return spawns.Count;
        }

        private static int BuildTowerSlots(LevelLayout layout, char[,] cells, Transform root)
        {
            List<Vector2Int> slots = FindSymbols(cells, SLOT_SYMBOL);
            Transform slotsRoot = CreateChild(root, "Tower Slots");
            for (int slotIndex = 0; slotIndex < slots.Count; slotIndex++)
            {
                Vector2Int slot = slots[slotIndex];
                Transform slotTransform = CreateChild(slotsRoot, "Tower Slot " + (slotIndex + 1));
                slotTransform.position = GetCellCenter(layout, slot.y, slot.x) + Vector3.up * layout.grassSurfaceHeight;
                slotTransform.gameObject.AddComponent<TowerSlotAuthoring>();
            }
            return slots.Count;
        }

        /// <summary>
        /// 씬의 CoreAuthoring을 코어 칸으로 옮긴다. 높이는 그대로 둔다 (코어 모델과 View가 같이 움직인다).
        /// </summary>
        private static bool MoveCore(LevelLayout layout, char[,] cells)
        {
            CoreAuthoring[] coreAuthorings = Object.FindObjectsByType<CoreAuthoring>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (coreAuthorings.Length == 0)
            {
                return false;
            }
            if (coreAuthorings.Length > 1)
            {
                Debug.LogWarning("[LevelLayoutBuilder] 씬에 CoreAuthoring이 " + coreAuthorings.Length + "개 있다. 첫 번째만 옮긴다.", coreAuthorings[0]);
            }

            Vector2Int core = FindSymbols(cells, CORE_SYMBOL)[0];
            Transform coreTransform = coreAuthorings[0].transform;
            Vector3 cellCenter = GetCellCenter(layout, core.y, core.x);
            Undo.RecordObject(coreTransform, "레벨 생성");
            coreTransform.position = new Vector3(cellCenter.x, coreTransform.position.y, cellCenter.z);
            return true;
        }

        /// <summary>
        /// 생성 루트 밖에 있는 스폰·경로·공터 Authoring을 지울지 묻고 지운다. SceneBaker는 씬의 모든 Authoring을 모으므로 남겨 두면 중복된다.
        /// 지울 수 없는 것(코어를 품은 오브젝트, 프리팹 내부)이 있거나 취소하면 false.
        /// </summary>
        private static bool RemoveAuthoringsOutside(GameObject root)
        {
            List<GameObject> targets = new List<GameObject>();
            CollectOutside(Object.FindObjectsByType<SpawnPointAuthoring>(FindObjectsInactive.Include, FindObjectsSortMode.None), root, targets);
            CollectOutside(Object.FindObjectsByType<WaypointPathAuthoring>(FindObjectsInactive.Include, FindObjectsSortMode.None), root, targets);
            CollectOutside(Object.FindObjectsByType<TowerSlotAuthoring>(FindObjectsInactive.Include, FindObjectsSortMode.None), root, targets);
            if (targets.Count == 0)
            {
                return true;
            }

            List<string> names = new List<string>();
            for (int targetIndex = 0; targetIndex < targets.Count; targetIndex++)
            {
                GameObject target = targets[targetIndex];
                if (target.GetComponentInChildren<CoreAuthoring>(true) != null)
                {
                    EditorUtility.DisplayDialog("레벨 생성", "'" + target.name + "' 아래에 CoreAuthoring이 있어 지울 수 없다. 코어를 다른 곳으로 옮긴 뒤 다시 시도한다.", "확인");
                    return false;
                }
                if (PrefabUtility.IsPartOfPrefabInstance(target) && !PrefabUtility.IsOutermostPrefabInstanceRoot(target))
                {
                    EditorUtility.DisplayDialog("레벨 생성", "'" + target.name + "'은(는) 프리팹 안에 있어 지울 수 없다. 프리팹에서 직접 지운 뒤 다시 시도한다.", "확인");
                    return false;
                }
                names.Add(target.name);
            }

            bool shouldRemove = EditorUtility.DisplayDialog(
                "레벨 생성",
                "씬에 기존 스폰·경로·공터 Authoring이 " + targets.Count + "개 있다. 남겨 두면 생성한 것과 중복된다. 지우고 새로 만들까?\n\n" + string.Join("\n", names),
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
        /// 생성 루트 밖의 Authoring 오브젝트를 모은다. 이미 모은 오브젝트의 자식이거나 부모이면 하나로 합친다.
        /// </summary>
        private static void CollectOutside<T>(T[] authorings, GameObject root, List<GameObject> targets) where T : MonoBehaviour
        {
            for (int authoringIndex = 0; authoringIndex < authorings.Length; authoringIndex++)
            {
                GameObject target = authorings[authoringIndex].gameObject;
                if (root != null && target.transform.IsChildOf(root.transform))
                {
                    continue;
                }

                bool isCovered = false;
                for (int targetIndex = targets.Count - 1; targetIndex >= 0; targetIndex--)
                {
                    Transform collected = targets[targetIndex].transform;
                    if (target.transform.IsChildOf(collected))
                    {
                        isCovered = true;
                        break;
                    }
                    if (collected.IsChildOf(target.transform))
                    {
                        targets.RemoveAt(targetIndex);
                    }
                }
                if (!isCovered)
                {
                    targets.Add(target);
                }
            }
        }

        private static GameObject FindRoot(Scene scene)
        {
            GameObject[] rootObjects = scene.GetRootGameObjects();
            for (int rootIndex = 0; rootIndex < rootObjects.Length; rootIndex++)
            {
                if (rootObjects[rootIndex].name == ROOT_NAME)
                {
                    return rootObjects[rootIndex];
                }
            }
            return null;
        }

        private static void ClearChildren(Transform parent)
        {
            for (int childIndex = parent.childCount - 1; childIndex >= 0; childIndex--)
            {
                Undo.DestroyObjectImmediate(parent.GetChild(childIndex).gameObject);
            }
        }

        private static Transform CreateChild(Transform parent, string childName)
        {
            GameObject child = new GameObject(childName);
            Undo.RegisterCreatedObjectUndo(child, "레벨 생성");
            child.transform.SetParent(parent, false);
            return child.transform;
        }

        private static void MarkBatchingStatic(Transform tile)
        {
            Transform[] transforms = tile.GetComponentsInChildren<Transform>(true);
            for (int transformIndex = 0; transformIndex < transforms.Length; transformIndex++)
            {
                GameObject target = transforms[transformIndex].gameObject;
                GameObjectUtility.SetStaticEditorFlags(target, GameObjectUtility.GetStaticEditorFlags(target) | StaticEditorFlags.BatchingStatic);
            }
        }

        private static GameObject GetTilePrefab(LevelLayout layout, LevelTileKind tileKind, out float yawOffset)
        {
            switch (tileKind)
            {
                case LevelTileKind.Straight:
                    yawOffset = layout.straightYawOffset;
                    return layout.straightPrefab;
                case LevelTileKind.Bend:
                    yawOffset = layout.bendYawOffset;
                    return layout.bendPrefab;
                case LevelTileKind.Split:
                    yawOffset = layout.splitYawOffset;
                    return layout.splitPrefab;
                case LevelTileKind.Cross:
                    yawOffset = layout.crossYawOffset;
                    return layout.crossPrefab;
                case LevelTileKind.End:
                    yawOffset = layout.endYawOffset;
                    return layout.endPrefab;
                default:
                    yawOffset = layout.grassYawOffset;
                    return layout.grassPrefab;
            }
        }

        /// <summary>
        /// 격자에 쓰인 타일 종류 중 프리팹이 비어 있는 것을 경고로 남긴다 (그 칸은 비워 둔다).
        /// </summary>
        private static void AddMissingPrefabWarnings(LevelLayout layout, char[,] cells, List<string> warnings)
        {
            bool[] isUsed = new bool[CountTileKinds()];
            for (int row = 0; row < cells.GetLength(0); row++)
            {
                for (int column = 0; column < cells.GetLength(1); column++)
                {
                    LevelTileKind tileKind;
                    int quarterTurns;
                    if (TryGetTile(cells, row, column, out tileKind, out quarterTurns))
                    {
                        isUsed[(int)tileKind] = true;
                    }
                }
            }

            for (int kindIndex = 0; kindIndex < isUsed.Length; kindIndex++)
            {
                float yawOffset;
                if (isUsed[kindIndex] && GetTilePrefab(layout, (LevelTileKind)kindIndex, out yawOffset) == null)
                {
                    warnings.Add(((LevelTileKind)kindIndex) + " 타일 프리팹이 비어 있다. 그 칸은 비워 둔다.");
                }
            }
        }

        /// <summary>
        /// 스폰에서 코어까지 길 칸만 밟는 가장 짧은 경로를 찾는다 (너비 우선 탐색, 이웃은 북·동·남·서 순서).
        /// </summary>
        /// <param name="route">스폰부터 코어까지의 칸 (x = 열, y = 행). 먼저 비운다</param>
        private static bool TryFindRoute(char[,] cells, Vector2Int start, Vector2Int goal, List<Vector2Int> route)
        {
            route.Clear();
            int rowCount = cells.GetLength(0);
            int columnCount = cells.GetLength(1);
            Vector2Int[,] cameFrom = new Vector2Int[rowCount, columnCount];
            bool[,] isVisited = new bool[rowCount, columnCount];
            Queue<Vector2Int> frontier = new Queue<Vector2Int>();
            frontier.Enqueue(start);
            isVisited[start.y, start.x] = true;

            while (frontier.Count > 0)
            {
                Vector2Int current = frontier.Dequeue();
                if (current == goal)
                {
                    for (Vector2Int step = goal; step != start; step = cameFrom[step.y, step.x])
                    {
                        route.Add(step);
                    }
                    route.Add(start);
                    route.Reverse();
                    return true;
                }

                for (int directionIndex = 0; directionIndex < ROW_STEPS.Length; directionIndex++)
                {
                    int nextRow = current.y + ROW_STEPS[directionIndex];
                    int nextColumn = current.x + COLUMN_STEPS[directionIndex];
                    if (nextRow < 0 || nextRow >= rowCount || nextColumn < 0 || nextColumn >= columnCount)
                    {
                        continue;
                    }
                    if (isVisited[nextRow, nextColumn] || !IsPathSymbol(cells[nextRow, nextColumn]))
                    {
                        continue;
                    }

                    isVisited[nextRow, nextColumn] = true;
                    cameFrom[nextRow, nextColumn] = current;
                    frontier.Enqueue(new Vector2Int(nextColumn, nextRow));
                }
            }
            return false;
        }

        /// <summary>
        /// 기호가 있는 칸을 위 줄부터, 왼쪽부터 차례로 모은다 (x = 열, y = 행).
        /// </summary>
        private static List<Vector2Int> FindSymbols(char[,] cells, char symbol)
        {
            List<Vector2Int> found = new List<Vector2Int>();
            for (int row = 0; row < cells.GetLength(0); row++)
            {
                for (int column = 0; column < cells.GetLength(1); column++)
                {
                    if (cells[row, column] == symbol)
                    {
                        found.Add(new Vector2Int(column, row));
                    }
                }
            }
            return found;
        }

        /// <summary>
        /// 이웃한 길 칸 방향을 비트로 모은다 (북 1, 동 2, 남 4, 서 8).
        /// </summary>
        private static int GetOpenMask(char[,] cells, int row, int column)
        {
            int openMask = 0;
            for (int directionIndex = 0; directionIndex < ROW_STEPS.Length; directionIndex++)
            {
                int neighborRow = row + ROW_STEPS[directionIndex];
                int neighborColumn = column + COLUMN_STEPS[directionIndex];
                if (neighborRow < 0 || neighborRow >= cells.GetLength(0) || neighborColumn < 0 || neighborColumn >= cells.GetLength(1))
                {
                    continue;
                }
                if (IsPathSymbol(cells[neighborRow, neighborColumn]))
                {
                    openMask |= 1 << directionIndex;
                }
            }
            return openMask;
        }

        /// <summary>
        /// 열린 방향을 시계 방향으로 90도 돌린다 (북 → 동 → 남 → 서 → 북).
        /// </summary>
        private static int RotateMaskClockwise(int openMask)
        {
            return ((openMask << 1) | (openMask >> 3)) & ALL_DIRECTIONS_MASK;
        }

        private static int CountBits(int openMask)
        {
            int count = 0;
            for (int bitMask = openMask; bitMask != 0; bitMask >>= 1)
            {
                count += bitMask & 1;
            }
            return count;
        }

        private static int CountTileKinds()
        {
            return global::System.Enum.GetValues(typeof(LevelTileKind)).Length;
        }

        private static bool IsKnownSymbol(char symbol)
        {
            return symbol == GRASS_SYMBOL || symbol == PATH_SYMBOL || symbol == SPAWN_SYMBOL
                || symbol == CORE_SYMBOL || symbol == SLOT_SYMBOL || symbol == EMPTY_SYMBOL;
        }

        // 풀밭
        public const char GRASS_SYMBOL = '.';
        // 길
        public const char PATH_SYMBOL = '#';
        // 적 스폰 (길 칸)
        public const char SPAWN_SYMBOL = 'S';
        // 코어 (길 칸)
        public const char CORE_SYMBOL = 'C';
        // 타워 공터 (풀밭 칸)
        public const char SLOT_SYMBOL = 'T';
        // 빈칸 (타일 없음)
        public const char EMPTY_SYMBOL = '-';
        // 생성물을 모아 두는 씬 루트 오브젝트 이름
        public const string ROOT_NAME = "Level (Generated)";

        private const int NORTH_MASK = 1;
        private const int EAST_MASK = 2;
        private const int SOUTH_MASK = 4;
        private const int WEST_MASK = 8;
        private const int ALL_DIRECTIONS_MASK = 15;
        private const float GROUND_COLLIDER_THICKNESS = 0.1f;
        // SpawnPointAuthoring의 경로 직렬화 필드 이름
        private const string SPAWN_PATH_PROPERTY_NAME = "_path";
    }
}
