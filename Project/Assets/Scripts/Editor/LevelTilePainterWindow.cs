using System.Collections.Generic;
using Project05.Authoring.Scene;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project05.Editor
{
    /// <summary>
    /// 레벨 칠하기 창 (Tools → Project05 → Level Tile Painter). 에디터 전용.
    /// 모드별로 Scene 뷰 클릭이 하는 일이 다르다. 타일: 칠하기·교체·지우기, 스폰·공터: 놓기·지우기, 코어: 옮기기.
    /// 칠한 타일은 "Level Tiles (Painted)", 스폰·공터는 "Level Markers" 아래에 둔다. 경로 생성(LevelPathBuilder)은 이 둘을 읽기만 한다.
    /// 클릭·드래그로 칠하고, Shift+클릭·드래그로 지우고, ] / [로 타일을 90도씩 돌린다 (R은 Unity 크기 조절 도구 단축키라 쓰지 않는다). Esc로 칠하기를 끈다.
    /// 한 번의 드래그는 Undo 한 번으로 되돌아간다. Scene 뷰 입력은 SceneView.duringSceneGui로 받는다 (에디터 전용이라 이벤트 금지 규칙 밖).
    /// </summary>
    public class LevelTilePainterWindow : EditorWindow
    {
        [SerializeField] private LevelTileSet _tileSet;
        [SerializeField] private LevelPaintMode _mode;
        [SerializeField] private int _selectedIndex;
        [SerializeField] private int _quarterTurns;
        [SerializeField] private bool _isPainting;

        private Vector2 _scrollPosition;
        private bool _isStroking;
        private bool _isErasingStroke;
        private int _strokeUndoGroup;
        private Vector2Int _lastStrokeCell;
        private bool _hasLastStrokeCell;
        // 창 가장자리 여백 스타일 (GUI 스킨이 준비된 뒤 OnGUI에서 한 번 만든다)
        private GUIStyle _paddingStyle;
        // 썸네일 목록 안쪽 여백 스타일 (선택 테두리가 스크롤 영역 가장자리에서 잘리지 않게)
        private GUIStyle _thumbnailAreaStyle;

        private static readonly string[] MODE_LABELS = { "타일", "스폰", "코어", "공터" };
        private static readonly Color SELECTED_COLOR = new Color(0.25f, 0.6f, 1f);
        private static readonly Color TILE_COLOR = new Color(0.3f, 0.9f, 1f);
        private static readonly Color SPAWN_COLOR = new Color(1f, 0.35f, 0.35f);
        private static readonly Color CORE_COLOR = new Color(0.35f, 0.55f, 1f);
        private static readonly Color SLOT_COLOR = new Color(1f, 0.85f, 0.25f);
        private static readonly Color ERASE_COLOR = new Color(1f, 0.2f, 0.2f);

        /// <summary>
        /// 창을 연다.
        /// </summary>
        [MenuItem("Tools/Project05/Level Tile Painter")]
        public static void Open()
        {
            GetWindow<LevelTilePainterWindow>("Level Tile Painter");
        }

        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private void OnGUI()
        {
            if (_paddingStyle == null)
            {
                _paddingStyle = new GUIStyle();
                _paddingStyle.padding = new RectOffset(WINDOW_PADDING, WINDOW_PADDING, WINDOW_PADDING, WINDOW_PADDING);
            }

            if (_thumbnailAreaStyle == null)
            {
                _thumbnailAreaStyle = new GUIStyle();
                _thumbnailAreaStyle.padding = new RectOffset(THUMBNAIL_AREA_PADDING, THUMBNAIL_AREA_PADDING, THUMBNAIL_AREA_PADDING, THUMBNAIL_AREA_PADDING);
            }

            EditorGUILayout.BeginVertical(_paddingStyle);
            DrawContent();
            EditorGUILayout.EndVertical();
        }

        private void DrawContent()
        {
            _tileSet = (LevelTileSet)EditorGUILayout.ObjectField("Level Tile Set", _tileSet, typeof(LevelTileSet), false);
            if (_tileSet == null)
            {
                GUILayout.Space(ELEMENT_SPACING);
                EditorGUILayout.HelpBox("격자 설정과 타일 목록을 담은 Level Tile Set 에셋을 넣는다.\n(Create → Project05 → Level Tile Set)", MessageType.Info);
                SetPainting(false);
                return;
            }

            GUILayout.Space(ELEMENT_SPACING);
            LevelPaintMode mode = (LevelPaintMode)GUILayout.Toolbar((int)_mode, MODE_LABELS, GUILayout.Height(MODE_BUTTON_HEIGHT));
            if (mode != _mode)
            {
                _mode = mode;
                SetPainting(true);
            }

            GUILayout.Space(ELEMENT_SPACING);
            string paintLabel = _isPainting ? "칠하기 끄기 (Esc)" : "칠하기 켜기";
            if (GUILayout.Button(paintLabel, GUILayout.Height(PAINT_BUTTON_HEIGHT)))
            {
                SetPainting(!_isPainting);
            }

            GUILayout.Space(ELEMENT_SPACING);
            if (_mode == LevelPaintMode.Tile)
            {
                EditorGUILayout.LabelField("현재 회전", (_quarterTurns * 90) + "°", EditorStyles.boldLabel);
                GUILayout.Space(ELEMENT_SPACING);
            }
            EditorGUILayout.HelpBox(GetControlsHelp(), MessageType.None);

            GUILayout.Space(ELEMENT_SPACING);
            if (GUILayout.Button("경로 생성 (칠한 길 타일 → 웨이포인트)", GUILayout.Height(PAINT_BUTTON_HEIGHT)))
            {
                LevelPathBuilder.Build(_tileSet);
            }

            if (_mode == LevelPaintMode.Tile)
            {
                GUILayout.Space(ELEMENT_SPACING);
                if (GUILayout.Button("이름에 'path'가 들어간 타일을 모두 길로 표시"))
                {
                    MarkPathPrefabsByName();
                }
                GUILayout.Space(ELEMENT_SPACING);
                DrawThumbnails();
            }
        }

        private string GetControlsHelp()
        {
            switch (_mode)
            {
                case LevelPaintMode.Spawn:
                    return "클릭 : 스폰 지점 놓기 (길 바닥 높이)\nShift + 클릭 : 그 칸의 스폰 지우기\nEsc : 끄기";
                case LevelPaintMode.Core:
                    return "클릭 : 코어를 그 칸으로 옮기기 (높이는 그대로)\nEsc : 끄기";
                case LevelPaintMode.TowerSlot:
                    return "클릭 : 타워 공터 놓기 (풀 바닥 높이)\nShift + 클릭 : 그 칸의 공터 지우기\nEsc : 끄기";
                default:
                    return TILE_CONTROLS_HELP;
            }
        }

        /// <summary>
        /// 타일 목록을 썸네일 격자로 그린다. 썸네일을 불러오는 중이면 다시 그린다.
        /// </summary>
        private void DrawThumbnails()
        {
            GameObject[] prefabs = _tileSet.prefabs;
            if (prefabs == null || prefabs.Length == 0)
            {
                EditorGUILayout.HelpBox("Level Tile Set의 prefabs가 비어 있다. Project 창에서 타일 FBX·프리팹을 끌어 넣는다.", MessageType.Warning);
                return;
            }

            _selectedIndex = Mathf.Clamp(_selectedIndex, 0, prefabs.Length - 1);
            float availableWidth = position.width - WINDOW_PADDING * 2f - SCROLLBAR_WIDTH - THUMBNAIL_AREA_PADDING * 2f;
            int columnCount = Mathf.Max(1, (int)((availableWidth + THUMBNAIL_SPACING) / (THUMBNAIL_SIZE + THUMBNAIL_SPACING)));
            int rowCount = (prefabs.Length + columnCount - 1) / columnCount;
            bool isLoading = false;

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            // 추가: 썸네일 목록 전체를 안쪽 여백으로 감싼다
            EditorGUILayout.BeginVertical(_thumbnailAreaStyle);
            for (int row = 0; row < rowCount; row++)
            {
                // 변경: 첫 줄이 아니면 위에 세로 간격 (마지막 줄 아래 간격은 여백이 맡는다)
                if (row > 0)
                {
                    GUILayout.Space(THUMBNAIL_SPACING);
                }
                EditorGUILayout.BeginHorizontal();
                for (int column = 0; column < columnCount; column++)
                {
                    int prefabIndex = row * columnCount + column;
                    if (prefabIndex >= prefabs.Length)
                    {
                        break;
                    }
                    // 첫 칸이 아니면 앞에 가로 간격
                    if (column > 0)
                    {
                        GUILayout.Space(THUMBNAIL_SPACING);
                    }
                    isLoading = DrawThumbnail(prefabs[prefabIndex], prefabIndex) || isLoading;
                }
                EditorGUILayout.EndHorizontal();
            }
            // 추가: 위에서 연 안쪽 여백을 닫는다
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();

            if (isLoading)
            {
                Repaint();
            }
        }

        /// <summary>
        /// 썸네일 하나 (이미지, 이름, 길 체크). 고르면 그 타일로 칠하기를 켜고, "길" 체크로 길 타일 여부를 바꾼다.
        /// 썸네일을 아직 불러오는 중이면 true.
        /// </summary>
        private bool DrawThumbnail(GameObject prefab, int prefabIndex)
        {
            Rect cellRect = GUILayoutUtility.GetRect(THUMBNAIL_SIZE, THUMBNAIL_SIZE + THUMBNAIL_INNER_SPACING + THUMBNAIL_LABEL_HEIGHT * 2f, GUILayout.Width(THUMBNAIL_SIZE));
            Rect imageRect = new Rect(cellRect.x, cellRect.y, THUMBNAIL_SIZE, THUMBNAIL_SIZE);
            Rect labelRect = new Rect(cellRect.x, imageRect.yMax + THUMBNAIL_INNER_SPACING, THUMBNAIL_SIZE, THUMBNAIL_LABEL_HEIGHT);
            Rect toggleRect = new Rect(cellRect.x + (THUMBNAIL_SIZE - PATH_TOGGLE_WIDTH) * 0.5f, labelRect.yMax, PATH_TOGGLE_WIDTH, THUMBNAIL_LABEL_HEIGHT);

            // 고른 타일은 썸네일 둘레에만 테두리를 그린다 (이름 글자가 강조색에 묻히지 않게)
            if (prefabIndex == _selectedIndex)
            {
                EditorGUI.DrawRect(new Rect(imageRect.x - SELECTED_BORDER, imageRect.y - SELECTED_BORDER, imageRect.width + SELECTED_BORDER * 2f, imageRect.height + SELECTED_BORDER * 2f), SELECTED_COLOR);
            }

            bool isLoading = false;
            Texture2D preview = null;
            if (prefab != null)
            {
                preview = AssetPreview.GetAssetPreview(prefab);
                if (preview == null)
                {
                    isLoading = AssetPreview.IsLoadingAssetPreview(prefab.GetInstanceID());
                    preview = AssetPreview.GetMiniThumbnail(prefab);
                }
            }

            if (GUI.Button(imageRect, new GUIContent(preview, prefab != null ? prefab.name : "(비어 있음)")))
            {
                _selectedIndex = prefabIndex;
                SetPainting(true);
            }
            GUI.Label(labelRect, prefab != null ? ShortenName(prefab.name) : "-", EditorStyles.centeredGreyMiniLabel);

            if (prefab != null)
            {
                bool isPath = LevelPathBuilder.IsPathPrefab(_tileSet, prefab);
                bool shouldBePath = GUI.Toggle(toggleRect, isPath, "길");
                if (shouldBePath != isPath)
                {
                    SetPathPrefab(prefab, shouldBePath);
                }
            }
            return isLoading;
        }

        private void SetPathPrefab(GameObject prefab, bool isPath)
        {
            List<GameObject> pathPrefabs = new List<GameObject>(_tileSet.pathPrefabs ?? new GameObject[0]);
            pathPrefabs.Remove(prefab);
            if (isPath)
            {
                pathPrefabs.Add(prefab);
            }

            Undo.RecordObject(_tileSet, "길 타일 표시");
            _tileSet.pathPrefabs = pathPrefabs.ToArray();
            EditorUtility.SetDirty(_tileSet);
        }

        private void MarkPathPrefabsByName()
        {
            GameObject[] prefabs = _tileSet.prefabs;
            for (int prefabIndex = 0; prefabIndex < prefabs.Length; prefabIndex++)
            {
                GameObject prefab = prefabs[prefabIndex];
                if (prefab != null && prefab.name.ToLowerInvariant().Contains(PATH_NAME_KEYWORD) && !LevelPathBuilder.IsPathPrefab(_tileSet, prefab))
                {
                    SetPathPrefab(prefab, true);
                }
            }
        }

        /// <summary>
        /// Scene 뷰 입력을 받아 모드에 맞게 처리한다. 칠하기가 켜져 있는 동안 왼쪽 클릭이 오브젝트 선택 대신 쓰인다.
        /// </summary>
        private void OnSceneGUI(SceneView sceneView)
        {
            if (!_isPainting || _tileSet == null)
            {
                return;
            }

            Event current = Event.current;
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            if (current.type == EventType.Layout)
            {
                HandleUtility.AddDefaultControl(controlID);
                return;
            }

            Vector2Int cell;
            bool hasCell = TryGetHoveredCell(current.mousePosition, out cell);
            switch (current.type)
            {
                case EventType.KeyDown:
                    HandleKey(current, sceneView);
                    break;
                case EventType.MouseDown:
                    if (current.button == 0 && !current.alt && hasCell)
                    {
                        GUIUtility.hotControl = controlID;
                        BeginStroke(current.shift);
                        ApplyStroke(cell);
                        current.Use();
                    }
                    break;
                case EventType.MouseDrag:
                    if (_isStroking && GUIUtility.hotControl == controlID)
                    {
                        // 스폰·코어·공터는 클릭한 칸 하나에만 놓는다 (드래그로 줄줄이 놓이지 않게)
                        if (hasCell && _mode == LevelPaintMode.Tile)
                        {
                            ApplyStroke(cell);
                        }
                        current.Use();
                    }
                    sceneView.Repaint();
                    break;
                case EventType.MouseUp:
                    if (_isStroking && current.button == 0)
                    {
                        EndStroke();
                        GUIUtility.hotControl = 0;
                        current.Use();
                    }
                    break;
                case EventType.MouseMove:
                    sceneView.Repaint();
                    break;
                case EventType.Repaint:
                    if (hasCell)
                    {
                        DrawCellPreview(cell, current.shift);
                    }
                    break;
            }
        }

        private void HandleKey(Event current, SceneView sceneView)
        {
            bool isRotateKey = current.keyCode == KeyCode.RightBracket || current.keyCode == KeyCode.LeftBracket;
            if (isRotateKey && _mode == LevelPaintMode.Tile)
            {
                _quarterTurns = (_quarterTurns + (current.keyCode == KeyCode.RightBracket ? 1 : 3)) % 4;
                current.Use();
                sceneView.Repaint();
                Repaint();
            }
            else if (current.keyCode == KeyCode.Escape)
            {
                SetPainting(false);
                current.Use();
            }
        }

        private void BeginStroke(bool isErasing)
        {
            Undo.IncrementCurrentGroup();
            _strokeUndoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("레벨 칠하기");
            _isStroking = true;
            _isErasingStroke = isErasing;
            _hasLastStrokeCell = false;
        }

        /// <summary>
        /// 칸이 바뀔 때마다 한 번씩 모드에 맞는 일을 한다.
        /// </summary>
        private void ApplyStroke(Vector2Int cell)
        {
            if (_hasLastStrokeCell && cell == _lastStrokeCell)
            {
                return;
            }
            _lastStrokeCell = cell;
            _hasLastStrokeCell = true;

            switch (_mode)
            {
                case LevelPaintMode.Tile:
                    if (_isErasingStroke)
                    {
                        EraseTile(cell);
                    }
                    else
                    {
                        PaintTile(cell);
                    }
                    break;
                case LevelPaintMode.Spawn:
                    PlaceOrEraseMarker<SpawnPointAuthoring>(cell, LevelPathBuilder.SPAWNS_GROUP_NAME, "Spawn", _tileSet.pathSurfaceHeight);
                    break;
                case LevelPaintMode.TowerSlot:
                    PlaceOrEraseMarker<TowerSlotAuthoring>(cell, LevelPathBuilder.TOWER_SLOTS_GROUP_NAME, "Tower Slot", _tileSet.grassSurfaceHeight);
                    break;
                case LevelPaintMode.Core:
                    if (!_isErasingStroke)
                    {
                        MoveCore(cell);
                    }
                    break;
            }
        }

        private void EndStroke()
        {
            Undo.CollapseUndoOperations(_strokeUndoGroup);
            _isStroking = false;
        }

        /// <summary>
        /// 칸에 고른 타일을 놓는다. 같은 타일이 같은 방향으로 이미 있으면 그대로 두고, 다른 타일이 있으면 교체한다.
        /// </summary>
        private void PaintTile(Vector2Int cell)
        {
            GameObject[] prefabs = _tileSet.prefabs;
            if (prefabs == null || _selectedIndex < 0 || _selectedIndex >= prefabs.Length || prefabs[_selectedIndex] == null)
            {
                return;
            }

            GameObject prefab = prefabs[_selectedIndex];
            Transform paintRoot = GetOrCreatePaintRoot();
            Vector3 center = LevelGrid.GetCellCenter(_tileSet, cell);
            Quaternion rotation = Quaternion.Euler(0f, _quarterTurns * 90f, 0f) * prefab.transform.rotation;

            GameObject existing = FindChildInCell(paintRoot, cell);
            if (existing != null)
            {
                bool isSameTile = PrefabUtility.GetCorrespondingObjectFromSource(existing) == prefab
                    && Quaternion.Angle(existing.transform.rotation, rotation) < SAME_ROTATION_DEGREES;
                if (isSameTile)
                {
                    return;
                }
                Undo.DestroyObjectImmediate(existing);
            }

            GameObject tile = (GameObject)PrefabUtility.InstantiatePrefab(prefab, paintRoot);
            Undo.RegisterCreatedObjectUndo(tile, "레벨 칠하기");
            tile.name = prefab.name + " " + cell.x + "," + cell.y;
            tile.transform.SetPositionAndRotation(center, rotation);
            tile.transform.localScale = prefab.transform.localScale * _tileSet.tileScale;
            if (_tileSet.markTilesStatic)
            {
                MarkBatchingStatic(tile.transform);
            }
        }

        private void EraseTile(Vector2Int cell)
        {
            GameObject paintRoot = FindRootObject(PAINT_ROOT_NAME);
            if (paintRoot == null)
            {
                return;
            }

            GameObject existing = FindChildInCell(paintRoot.transform, cell);
            if (existing != null)
            {
                Undo.DestroyObjectImmediate(existing);
            }
        }

        /// <summary>
        /// 칸에 표시(스폰·공터)를 놓거나 지운다. 같은 칸에 같은 종류가 있으면 놓지 않는다.
        /// 지우기는 직접 만든 것까지 씬 전체에서 찾아 그 칸의 것을 지운다.
        /// </summary>
        private void PlaceOrEraseMarker<T>(Vector2Int cell, string groupName, string markerName, float surfaceHeight) where T : MonoBehaviour
        {
            T existing = FindMarkerInCell<T>(cell);
            if (_isErasingStroke)
            {
                if (existing != null)
                {
                    Undo.DestroyObjectImmediate(existing.gameObject);
                }
                return;
            }
            if (existing != null)
            {
                return;
            }

            Transform group = LevelPathBuilder.GetOrCreateMarkerGroup(groupName);
            GameObject marker = new GameObject(markerName + " " + cell.x + "," + cell.y);
            Undo.RegisterCreatedObjectUndo(marker, "레벨 칠하기");
            marker.transform.SetParent(group, false);
            marker.transform.position = LevelGrid.GetCellCenter(_tileSet, cell) + Vector3.up * surfaceHeight;
            marker.AddComponent<T>();
        }

        private T FindMarkerInCell<T>(Vector2Int cell) where T : MonoBehaviour
        {
            T[] markers = Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int markerIndex = 0; markerIndex < markers.Length; markerIndex++)
            {
                if (LevelGrid.GetCell(_tileSet, markers[markerIndex].transform.position) == cell)
                {
                    return markers[markerIndex];
                }
            }
            return null;
        }

        /// <summary>
        /// 씬의 CoreAuthoring을 칸 중심으로 옮긴다. 높이는 그대로 둔다 (코어 모델과 View가 같이 움직인다).
        /// </summary>
        private void MoveCore(Vector2Int cell)
        {
            CoreAuthoring[] cores = Object.FindObjectsByType<CoreAuthoring>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (cores.Length == 0)
            {
                Debug.LogWarning("[LevelTilePainterWindow] 씬에 CoreAuthoring이 없어 코어를 옮길 수 없다.");
                return;
            }

            Transform coreTransform = cores[0].transform;
            Vector3 center = LevelGrid.GetCellCenter(_tileSet, cell);
            Undo.RecordObject(coreTransform, "레벨 칠하기");
            coreTransform.position = new Vector3(center.x, coreTransform.position.y, center.z);
        }

        /// <summary>
        /// 마우스가 가리키는 바닥(origin 높이의 수평면) 위 칸을 구한다.
        /// </summary>
        private bool TryGetHoveredCell(Vector2 mousePosition, out Vector2Int cell)
        {
            cell = Vector2Int.zero;
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
            Plane ground = new Plane(Vector3.up, new Vector3(0f, _tileSet.origin.y, 0f));
            float distance;
            if (!ground.Raycast(ray, out distance))
            {
                return false;
            }

            cell = LevelGrid.GetCell(_tileSet, ray.GetPoint(distance));
            return true;
        }

        /// <summary>
        /// 가리킨 칸 테두리를 모드 색으로 그린다. 지우기면 빨간색, 타일 모드면 회전 방향(타일의 +Z 쪽) 선도 그린다.
        /// </summary>
        private void DrawCellPreview(Vector2Int cell, bool isErasing)
        {
            Vector3 center = LevelGrid.GetCellCenter(_tileSet, cell);
            float half = _tileSet.cellSize * 0.5f;
            Vector3[] corners =
            {
                center + new Vector3(-half, 0f, -half),
                center + new Vector3(-half, 0f, half),
                center + new Vector3(half, 0f, half),
                center + new Vector3(half, 0f, -half),
            };
            bool canErase = _mode != LevelPaintMode.Core;
            Color outline = isErasing && canErase ? ERASE_COLOR : GetModeColor();
            Color fill = new Color(outline.r, outline.g, outline.b, PREVIEW_FILL_ALPHA);
            Handles.DrawSolidRectangleWithOutline(corners, fill, outline);

            if (_mode == LevelPaintMode.Tile && !isErasing)
            {
                Vector3 forward = Quaternion.Euler(0f, _quarterTurns * 90f, 0f) * Vector3.forward;
                Handles.color = outline;
                Handles.DrawLine(center, center + forward * half);
            }
        }

        private Color GetModeColor()
        {
            switch (_mode)
            {
                case LevelPaintMode.Spawn:
                    return SPAWN_COLOR;
                case LevelPaintMode.Core:
                    return CORE_COLOR;
                case LevelPaintMode.TowerSlot:
                    return SLOT_COLOR;
                default:
                    return TILE_COLOR;
            }
        }

        private void SetPainting(bool isPainting)
        {
            if (_isPainting == isPainting)
            {
                return;
            }

            _isPainting = isPainting;
            if (!isPainting && _isStroking)
            {
                EndStroke();
            }
            SceneView.RepaintAll();
            Repaint();
        }

        private GameObject FindChildInCell(Transform root, Vector2Int cell)
        {
            for (int childIndex = 0; childIndex < root.childCount; childIndex++)
            {
                Transform child = root.GetChild(childIndex);
                if (LevelGrid.GetCell(_tileSet, child.position) == cell)
                {
                    return child.gameObject;
                }
            }
            return null;
        }

        private static GameObject FindRootObject(string objectName)
        {
            GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
            for (int rootIndex = 0; rootIndex < rootObjects.Length; rootIndex++)
            {
                if (rootObjects[rootIndex].name == objectName)
                {
                    return rootObjects[rootIndex];
                }
            }
            return null;
        }

        private static Transform GetOrCreatePaintRoot()
        {
            GameObject paintRoot = FindRootObject(PAINT_ROOT_NAME);
            if (paintRoot == null)
            {
                paintRoot = new GameObject(PAINT_ROOT_NAME);
                Undo.RegisterCreatedObjectUndo(paintRoot, "레벨 칠하기");
            }
            return paintRoot.transform;
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

        /// <summary>
        /// 썸네일 아래 이름에서 "ground_" 같은 앞부분을 떼어 짧게 보여 준다.
        /// </summary>
        private static string ShortenName(string prefabName)
        {
            int separatorIndex = prefabName.IndexOf('_');
            return separatorIndex >= 0 && separatorIndex < prefabName.Length - 1 ? prefabName.Substring(separatorIndex + 1) : prefabName;
        }

        // 칠한 타일을 모아 두는 씬 루트 오브젝트 이름 (경로 생성이 지우지 않는다)
        public const string PAINT_ROOT_NAME = "Level Tiles (Painted)";

        private const string TILE_CONTROLS_HELP =
            "클릭 / 드래그 : 칠하기 (같은 칸은 교체)\n" +
            "Shift + 클릭 / 드래그 : 지우기\n" +
            "오른쪽 대괄호 키 ] : 시계 방향으로 90도 회전\n" +
            "왼쪽 대괄호 키 [ : 반시계 방향으로 90도 회전\n" +
            "Esc : 칠하기 끄기\n" +
            "Alt + 드래그, 우클릭 드래그 : Scene 뷰 카메라 조작 (그대로)";
        // 이름으로 길 타일을 고를 때 찾는 글자 (소문자)
        private const string PATH_NAME_KEYWORD = "path";

        private const float MODE_BUTTON_HEIGHT = 26f;
        private const float PAINT_BUTTON_HEIGHT = 28f;
        private const float SELECTED_BORDER = 3f;
        private const float THUMBNAIL_SIZE = 72f;
        private const float THUMBNAIL_SPACING = 10f;
        private const float THUMBNAIL_LABEL_HEIGHT = 16f;
        private const float PREVIEW_FILL_ALPHA = 0.15f;
        // 같은 방향으로 보는 회전 차이 (도)
        private const float SAME_ROTATION_DEGREES = 1f;
        // 창 가장자리 여백 (픽셀)
        private const int WINDOW_PADDING = 10;
        // 요소 사이 세로 간격 (픽셀)
        private const float ELEMENT_SPACING = 10f;
        // 썸네일 그림과 이름 사이 간격
        private const float THUMBNAIL_INNER_SPACING = 2f;
        // "길" 체크 너비 (썸네일 가운데 놓기용)
        private const float PATH_TOGGLE_WIDTH = 36f;
        // 세로 스크롤바 너비 (열 개수 계산에서 뺀다)
        private const float SCROLLBAR_WIDTH = 16f;
        // 썸네일 목록 안쪽 여백 (픽셀, 선택 테두리 SELECTED_BORDER보다 커야 잘리지 않는다)
        private const int THUMBNAIL_AREA_PADDING = 15;
    }
}
