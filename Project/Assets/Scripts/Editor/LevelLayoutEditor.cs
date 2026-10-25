using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Project05.Editor
{
    /// <summary>
    /// LevelLayout 인스펙터. 기호 설명, 격자 미리보기(칸 색), 검사 결과, 레벨 생성 버튼을 보여 준다.
    /// </summary>
    [CustomEditor(typeof(LevelLayout))]
    public class LevelLayoutEditor : UnityEditor.Editor
    {
        private readonly List<string> _errors = new List<string>();
        private readonly List<string> _warnings = new List<string>();

        private static readonly Color GRASS_COLOR = new Color(0.35f, 0.75f, 0.55f);
        private static readonly Color PATH_COLOR = new Color(0.72f, 0.5f, 0.35f);
        private static readonly Color SPAWN_COLOR = new Color(0.9f, 0.25f, 0.25f);
        private static readonly Color CORE_COLOR = new Color(0.25f, 0.45f, 0.95f);
        private static readonly Color SLOT_COLOR = new Color(0.95f, 0.85f, 0.25f);
        private static readonly Color EMPTY_COLOR = new Color(0.15f, 0.15f, 0.15f);

        /// <summary>
        /// 인스펙터를 그린다. 오류가 있으면 생성 버튼을 막는다.
        /// </summary>
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox(SYMBOL_HELP, MessageType.None);
            DrawDefaultInspector();

            LevelLayout layout = (LevelLayout)target;
            LevelLayoutBuilder.Validate(layout, _errors, _warnings);

            EditorGUILayout.Space();
            DrawPreview(layout);

            for (int errorIndex = 0; errorIndex < _errors.Count; errorIndex++)
            {
                EditorGUILayout.HelpBox(_errors[errorIndex], MessageType.Error);
            }
            for (int warningIndex = 0; warningIndex < _warnings.Count; warningIndex++)
            {
                EditorGUILayout.HelpBox(_warnings[warningIndex], MessageType.Warning);
            }

            EditorGUILayout.Space();
            using (new EditorGUI.DisabledScope(_errors.Count > 0))
            {
                if (GUILayout.Button("레벨 생성 (지금 열린 씬)", GUILayout.Height(BUTTON_HEIGHT)))
                {
                    LevelLayoutBuilder.Build(layout);
                }
            }
        }

        /// <summary>
        /// 격자를 칸 색으로 그린다. 위가 북쪽(+Z)이다. 길 칸에는 고른 타일 종류의 첫 글자를 적는다.
        /// </summary>
        private void DrawPreview(LevelLayout layout)
        {
            char[,] cells = LevelLayoutBuilder.ParseGrid(layout.grid, new List<string>(), new List<string>());
            if (cells == null)
            {
                return;
            }

            int rowCount = cells.GetLength(0);
            int columnCount = cells.GetLength(1);
            float cellPixels = Mathf.Clamp((EditorGUIUtility.currentViewWidth - PREVIEW_MARGIN) / columnCount, MIN_CELL_PIXELS, MAX_CELL_PIXELS);
            Rect area = GUILayoutUtility.GetRect(columnCount * cellPixels, rowCount * cellPixels, GUILayout.ExpandWidth(false));

            for (int row = 0; row < rowCount; row++)
            {
                for (int column = 0; column < columnCount; column++)
                {
                    Rect cellRect = new Rect(area.x + column * cellPixels, area.y + row * cellPixels, cellPixels - 1f, cellPixels - 1f);
                    char symbol = cells[row, column];
                    EditorGUI.DrawRect(cellRect, GetPreviewColor(symbol));

                    LevelTileKind tileKind;
                    int quarterTurns;
                    if (LevelLayoutBuilder.IsPathSymbol(symbol) && LevelLayoutBuilder.TryGetTile(cells, row, column, out tileKind, out quarterTurns))
                    {
                        GUI.Label(cellRect, GetPreviewLabel(tileKind), EditorStyles.centeredGreyMiniLabel);
                    }
                }
            }
        }

        /// <summary>
        /// 타일 종류를 모양이 닮은 글자로 보여 준다 (I 직선, L 굽은 길, T 갈림길, + 교차로, E 막다른 길).
        /// </summary>
        private static string GetPreviewLabel(LevelTileKind tileKind)
        {
            switch (tileKind)
            {
                case LevelTileKind.Straight:
                    return "I";
                case LevelTileKind.Bend:
                    return "L";
                case LevelTileKind.Split:
                    return "T";
                case LevelTileKind.Cross:
                    return "+";
                case LevelTileKind.End:
                    return "E";
                default:
                    return string.Empty;
            }
        }

        private static Color GetPreviewColor(char symbol)
        {
            switch (symbol)
            {
                case LevelLayoutBuilder.GRASS_SYMBOL:
                    return GRASS_COLOR;
                case LevelLayoutBuilder.PATH_SYMBOL:
                    return PATH_COLOR;
                case LevelLayoutBuilder.SPAWN_SYMBOL:
                    return SPAWN_COLOR;
                case LevelLayoutBuilder.CORE_SYMBOL:
                    return CORE_COLOR;
                case LevelLayoutBuilder.SLOT_SYMBOL:
                    return SLOT_COLOR;
                default:
                    return EMPTY_COLOR;
            }
        }

        private const string SYMBOL_HELP =
            "기호: '.' 풀밭  '#' 길  'S' 적 스폰  'C' 코어  'T' 타워 공터  '-' 빈칸 (공백 무시)\n" +
            "첫 줄이 북쪽(+Z), 왼쪽이 서쪽(-X). 길 칸은 이웃한 길을 보고 타일 종류와 방향을 자동으로 고른다.\n" +
            "미리보기 글자: I 직선, L 굽은 길, T 갈림길, + 교차로, E 막다른 길";

        private const float BUTTON_HEIGHT = 32f;
        private const float PREVIEW_MARGIN = 40f;
        private const float MIN_CELL_PIXELS = 8f;
        private const float MAX_CELL_PIXELS = 28f;
    }
}
