using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project05.Editor
{
    /// <summary>
    /// 타일 칠하기 창 (Tools → Project05 → Level Tile Painter). 에디터 전용.
    /// LevelTileSet의 프리팹을 썸네일로 골라 Scene 뷰에서 LevelLayout과 같은 격자 칸에 칠한다.
    /// 클릭·드래그로 칠하고, Shift+클릭·드래그로 지우고, ] / [로 90도씩 돌린다 (R은 Unity 크기 조절 도구 단축키라 쓰지 않는다). Esc로 칠하기를 끈다.
    /// 칠한 타일은 "Level Tiles (Painted)" 아래에 두므로 레벨 생성(LevelLayoutBuilder)으로 다시 만들어도 지워지지 않는다.
    /// 같은 칸에 다시 칠하면 교체하고, 한 번의 드래그는 Undo 한 번으로 되돌아간다.
    /// </summary>
    public class LevelTilePainterWindow : EditorWindow
    {
        [SerializeField] private LevelLayout _layout;
        [SerializeField] private LevelTileSet _tileSet;
        [SerializeField] private int _selectedIndex;
        [SerializeField] private int _quarterTurns;
        [SerializeField] private bool _isPainting;

        private Vector2 _scrollPosition;
        private bool _isStroking;
        private bool _isErasingStroke;
        private int _strokeUndoGroup;
        private Vector2Int _lastStrokeCell;
        private bool _hasLastStrokeCell;

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
            _layout = (LevelLayout)EditorGUILayout.ObjectField("격자 (Level Layout)", _layout, typeof(LevelLayout), false);
            _tileSet = (LevelTileSet)EditorGUILayout.ObjectField("타일 목록 (Level Tile Set)", _tileSet, typeof(LevelTileSet), false);
            if (_layout == null || _tileSet == null)
            {
                EditorGUILayout.HelpBox("격자 칸 크기·원점은 Level Layout 에셋에서, 칠할 타일은 Level Tile Set 에셋에서 가져온다. 둘 다 넣는다.\n(Create → Project05 → Level Layout / Level Tile Set)", MessageType.Info);
                SetPainting(false);
                return;
            }

            EditorGUILayout.Space();
            string paintLabel = _isPainting ? "칠하기 끄기 (Esc)" : "칠하기 켜기";
            if (GUILayout.Button(paintLabel, GUILayout.Height(PAINT_BUTTON_HEIGHT)))
            {
                SetPainting(!_isPainting);
            }
            EditorGUILayout.LabelField("현재 회전", (_quarterTurns * 90) + "°", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(CONTROLS_HELP, MessageType.None);

            DrawThumbnails();
        }

        /// <summary>
        /// 타일 목록을 썸네일 격자로 그린다. 고르면 그 타일로 칠하기를 켠다. 썸네일을 불러오는 중이면 다시 그린다.
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
            int columnCount = Mathf.Max(1, (int)((position.width - THUMBNAIL_MARGIN) / (THUMBNAIL_SIZE + THUMBNAIL_SPACING)));
            int rowCount = (prefabs.Length + columnCount - 1) / columnCount;
            bool isLoading = false;

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            for (int row = 0; row < rowCount; row++)
            {
                EditorGUILayout.BeginHorizontal();
                for (int column = 0; column < columnCount; column++)
                {
                    int prefabIndex = row * columnCount + column;
                    if (prefabIndex >= prefabs.Length)
                    {
                        break;
                    }

                    GameObject prefab = prefabs[prefabIndex];
                    Rect cellRect = GUILayoutUtility.GetRect(THUMBNAIL_SIZE, THUMBNAIL_SIZE + THUMBNAIL_LABEL_HEIGHT, GUILayout.Width(THUMBNAIL_SIZE));
                    Rect imageRect = new Rect(cellRect.x, cellRect.y, THUMBNAIL_SIZE, THUMBNAIL_SIZE);
                    Rect labelRect = new Rect(cellRect.x, cellRect.y + THUMBNAIL_SIZE, THUMBNAIL_SIZE, THUMBNAIL_LABEL_HEIGHT);

                    // 고른 타일은 썸네일 둘레에만 테두리를 그린다 (이름 글자가 강조색에 묻히지 않게)
                    if (prefabIndex == _selectedIndex)
                    {
                        EditorGUI.DrawRect(new Rect(imageRect.x - SELECTED_BORDER, imageRect.y - SELECTED_BORDER, imageRect.width + SELECTED_BORDER * 2f, imageRect.height + SELECTED_BORDER * 2f), SELECTED_COLOR);
                    }

                    Texture2D preview = null;
                    if (prefab != null)
                    {
                        preview = AssetPreview.GetAssetPreview(prefab);
                        if (preview == null)
                        {
                            isLoading = isLoading || AssetPreview.IsLoadingAssetPreview(prefab.GetInstanceID());
                            preview = AssetPreview.GetMiniThumbnail(prefab);
                        }
                    }

                    if (GUI.Button(imageRect, new GUIContent(preview, prefab != null ? prefab.name : "(비어 있음)")))
                    {
                        _selectedIndex = prefabIndex;
                        SetPainting(true);
                    }
                    GUI.Label(labelRect, prefab != null ? ShortenName(prefab.name) : "-", EditorStyles.centeredGreyMiniLabel);
                }
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(THUMBNAIL_SPACING);
            }
            EditorGUILayout.EndScrollView();

            if (isLoading)
            {
                Repaint();
            }
        }

        /// <summary>
        /// Scene 뷰 입력을 받아 칠하고 지운다. 칠하기가 켜져 있는 동안 왼쪽 클릭이 오브젝트 선택 대신 칠하기로 쓰인다.
        /// </summary>
        private void OnSceneGUI(SceneView sceneView)
        {
            if (!_isPainting || _layout == null || _tileSet == null)
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
                        if (hasCell)
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
            if (current.keyCode == KeyCode.RightBracket || current.keyCode == KeyCode.LeftBracket)
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
            Undo.SetCurrentGroupName(isErasing ? "타일 지우기" : "타일 칠하기");
            _isStroking = true;
            _isErasingStroke = isErasing;
            _hasLastStrokeCell = false;
        }

        /// <summary>
        /// 드래그 중 칸이 바뀔 때마다 한 번씩 칠하거나 지운다.
        /// </summary>
        private void ApplyStroke(Vector2Int cell)
        {
            if (_hasLastStrokeCell && cell == _lastStrokeCell)
            {
                return;
            }
            _lastStrokeCell = cell;
            _hasLastStrokeCell = true;

            if (_isErasingStroke)
            {
                EraseCell(cell);
            }
            else
            {
                PaintCell(cell);
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
        private void PaintCell(Vector2Int cell)
        {
            GameObject[] prefabs = _tileSet.prefabs;
            if (prefabs == null || _selectedIndex < 0 || _selectedIndex >= prefabs.Length || prefabs[_selectedIndex] == null)
            {
                return;
            }

            GameObject prefab = prefabs[_selectedIndex];
            Transform root = GetOrCreatePaintRoot();
            Vector3 center = GetCellCenter(cell);
            Quaternion rotation = Quaternion.Euler(0f, _quarterTurns * 90f, 0f) * prefab.transform.rotation;

            GameObject existing = FindTileAt(root, center);
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

            GameObject tile = (GameObject)PrefabUtility.InstantiatePrefab(prefab, root);
            Undo.RegisterCreatedObjectUndo(tile, "타일 칠하기");
            tile.name = prefab.name + " " + (cell.y + 1) + "," + (cell.x + 1);
            tile.transform.SetPositionAndRotation(center, rotation);
            tile.transform.localScale = prefab.transform.localScale * _layout.tileScale;
            if (_layout.markTilesStatic)
            {
                MarkBatchingStatic(tile.transform);
            }
        }

        private void EraseCell(Vector2Int cell)
        {
            GameObject paintRoot = FindPaintRoot();
            if (paintRoot == null)
            {
                return;
            }

            GameObject existing = FindTileAt(paintRoot.transform, GetCellCenter(cell));
            if (existing != null)
            {
                Undo.DestroyObjectImmediate(existing);
            }
        }

        /// <summary>
        /// 마우스가 가리키는 바닥(origin 높이의 수평면) 위 칸을 구한다. 칸 번호는 LevelLayout 격자와 같다 (x = 열, y = 행, 격자 밖이면 음수도 된다).
        /// </summary>
        private bool TryGetHoveredCell(Vector2 mousePosition, out Vector2Int cell)
        {
            cell = Vector2Int.zero;
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
            Plane ground = new Plane(Vector3.up, new Vector3(0f, _layout.origin.y, 0f));
            float distance;
            if (!ground.Raycast(ray, out distance))
            {
                return false;
            }

            Vector3 point = ray.GetPoint(distance);
            int column = Mathf.RoundToInt((point.x - _layout.origin.x) / _layout.cellSize);
            int row = Mathf.RoundToInt((_layout.origin.z - point.z) / _layout.cellSize);
            cell = new Vector2Int(column, row);
            return true;
        }

        private Vector3 GetCellCenter(Vector2Int cell)
        {
            return _layout.origin + new Vector3(cell.x * _layout.cellSize, 0f, -cell.y * _layout.cellSize);
        }

        /// <summary>
        /// 가리킨 칸 테두리와 회전 방향(칸 중심에서 타일의 +Z 쪽)을 그린다. 지우기면 빨간색.
        /// </summary>
        private void DrawCellPreview(Vector2Int cell, bool isErasing)
        {
            Vector3 center = GetCellCenter(cell);
            float half = _layout.cellSize * 0.5f;
            Vector3[] corners =
            {
                center + new Vector3(-half, 0f, -half),
                center + new Vector3(-half, 0f, half),
                center + new Vector3(half, 0f, half),
                center + new Vector3(half, 0f, -half),
            };
            Color outline = isErasing ? ERASE_COLOR : PAINT_COLOR;
            Color fill = new Color(outline.r, outline.g, outline.b, PREVIEW_FILL_ALPHA);
            Handles.DrawSolidRectangleWithOutline(corners, fill, outline);

            if (!isErasing)
            {
                Vector3 forward = Quaternion.Euler(0f, _quarterTurns * 90f, 0f) * Vector3.forward;
                Handles.color = PAINT_COLOR;
                Handles.DrawLine(center, center + forward * half);
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

        private static GameObject FindTileAt(Transform root, Vector3 center)
        {
            for (int childIndex = 0; childIndex < root.childCount; childIndex++)
            {
                Transform child = root.GetChild(childIndex);
                float deltaX = child.position.x - center.x;
                float deltaZ = child.position.z - center.z;
                if (deltaX * deltaX + deltaZ * deltaZ < SAME_CELL_SQR_DISTANCE)
                {
                    return child.gameObject;
                }
            }
            return null;
        }

        private static GameObject FindPaintRoot()
        {
            GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
            for (int rootIndex = 0; rootIndex < rootObjects.Length; rootIndex++)
            {
                if (rootObjects[rootIndex].name == PAINT_ROOT_NAME)
                {
                    return rootObjects[rootIndex];
                }
            }
            return null;
        }

        private static Transform GetOrCreatePaintRoot()
        {
            GameObject paintRoot = FindPaintRoot();
            if (paintRoot == null)
            {
                paintRoot = new GameObject(PAINT_ROOT_NAME);
                Undo.RegisterCreatedObjectUndo(paintRoot, "타일 칠하기");
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

        // 칠한 타일을 모아 두는 씬 루트 오브젝트 이름 (레벨 생성의 "Level (Generated)"와 따로 둔다)
        public const string PAINT_ROOT_NAME = "Level Tiles (Painted)";

        private static readonly Color SELECTED_COLOR = new Color(0.25f, 0.6f, 1f);
        private static readonly Color PAINT_COLOR = new Color(0.3f, 0.9f, 1f);
        private static readonly Color ERASE_COLOR = new Color(1f, 0.3f, 0.3f);

        private const string CONTROLS_HELP =
            "클릭 / 드래그 : 칠하기 (같은 칸은 교체)\n" +
            "Shift + 클릭 / 드래그 : 지우기\n" +
            "오른쪽 대괄호 키 ] : 시계 방향으로 90도 회전\n" +
            "왼쪽 대괄호 키 [ : 반시계 방향으로 90도 회전\n" +
            "Esc : 칠하기 끄기\n" +
            "Alt + 드래그, 우클릭 드래그 : Scene 뷰 카메라 조작 (그대로)";

        private const float PAINT_BUTTON_HEIGHT = 28f;
        private const float SELECTED_BORDER = 3f;
        private const float THUMBNAIL_SIZE = 72f;
        private const float THUMBNAIL_SPACING = 6f;
        private const float THUMBNAIL_LABEL_HEIGHT = 16f;
        private const float THUMBNAIL_MARGIN = 24f;
        private const float PREVIEW_FILL_ALPHA = 0.15f;
        // 같은 칸으로 보는 거리 제곱 (칸 크기와 상관없이 칸 중심끼리는 0에 가깝다)
        private const float SAME_CELL_SQR_DISTANCE = 0.01f;
        // 같은 방향으로 보는 회전 차이 (도)
        private const float SAME_ROTATION_DEGREES = 1f;
    }
}
