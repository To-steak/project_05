using System.Collections.Generic;
using Project05.Authoring.Definition;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Project05.Editor
{
    /// <summary>
    /// 빌드 직전에 빌드 플랫폼에 맞지 않는 히어로 프리팹 칸을 비워, 그 프리팹과 텍스처가 빌드에 들어가지 않게 한다.
    /// WebGL 빌드는 desktopPrefab(PBR)을, 그 밖의 빌드는 webPrefab(Poly)을 비운다. 빌드가 끝나면 되돌린다.
    /// GameDefinition은 Resources에서 불러오므로 참조가 남아 있으면 쓰지 않는 프리팹도 빌드에 들어간다.
    /// 빌드가 실패해 후처리가 불리지 않아도 다음 에디터 프레임(delayCall)에 되돌린다.
    /// 아래 static 필드는 빌드 한 번 동안의 에디터 작업 상태이며 게임 상태가 아니다.
    /// </summary>
    public class HeroPrefabBuildProcessor : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        // 비운 칸이 있는 히어로 정의 (되돌릴 대상)
        private static readonly List<HeroDefinition> _clearedDefinitions = new List<HeroDefinition>();
        // 비우기 전 프리팹 (_clearedDefinitions와 같은 순서)
        private static readonly List<GameObject> _clearedPrefabs = new List<GameObject>();
        // 이번 빌드에서 webPrefab을 비웠는지 (false면 desktopPrefab을 비웠다)
        private static bool _hasClearedWebPrefab;

        /// <summary>
        /// 다른 빌드 처리기보다 먼저 실행할 순서.
        /// </summary>
        public int callbackOrder
        {
            get { return 0; }
        }

        /// <summary>
        /// 빌드 직전에 빌드 플랫폼에 맞지 않는 히어로 프리팹 칸을 비우고 저장한다.
        /// </summary>
        /// <param name="report">빌드 보고서 (빌드 플랫폼을 읽는다)</param>
        public void OnPreprocessBuild(BuildReport report)
        {
            // 이전 빌드가 되돌리지 못한 칸이 있으면 먼저 되돌린다
            Restore();

            _hasClearedWebPrefab = report.summary.platform != BuildTarget.WebGL;
            string[] guids = AssetDatabase.FindAssets("t:" + nameof(HeroDefinition));
            for (int guidIndex = 0; guidIndex < guids.Length; guidIndex++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[guidIndex]);
                HeroDefinition definition = AssetDatabase.LoadAssetAtPath<HeroDefinition>(path);
                if (definition == null)
                {
                    continue;
                }

                GameObject clearedPrefab = _hasClearedWebPrefab ? definition.webPrefab : definition.desktopPrefab;
                if (clearedPrefab == null)
                {
                    continue;
                }

                GameObject keptPrefab = _hasClearedWebPrefab ? definition.desktopPrefab : definition.webPrefab;
                if (keptPrefab == null)
                {
                    Debug.LogWarning("[HeroPrefabBuildProcessor] " + definition.name + "에 이번 플랫폼용 히어로 프리팹이 없다. 다른 플랫폼용 " + clearedPrefab.name + "을(를) 그대로 빌드에 넣는다.", definition);
                    continue;
                }

                _clearedDefinitions.Add(definition);
                _clearedPrefabs.Add(clearedPrefab);
                SetClearedField(definition, null);
            }

            if (_clearedDefinitions.Count == 0)
            {
                return;
            }

            AssetDatabase.SaveAssets();
            EditorApplication.delayCall += Restore;
            Debug.Log("[HeroPrefabBuildProcessor] 빌드 동안 " + (_hasClearedWebPrefab ? "webPrefab" : "desktopPrefab") + " 칸을 비운다 (" + _clearedDefinitions.Count + "개 정의).");
        }

        /// <summary>
        /// 빌드가 끝나면 비운 히어로 프리팹 칸을 되돌리고 저장한다.
        /// </summary>
        /// <param name="report">빌드 보고서 (쓰지 않는다)</param>
        public void OnPostprocessBuild(BuildReport report)
        {
            Restore();
        }

        /// <summary>
        /// 비운 칸을 비우기 전 프리팹으로 되돌린다. 되돌릴 것이 없으면 아무것도 하지 않는다.
        /// </summary>
        private static void Restore()
        {
            EditorApplication.delayCall -= Restore;
            if (_clearedDefinitions.Count == 0)
            {
                return;
            }

            for (int definitionIndex = 0; definitionIndex < _clearedDefinitions.Count; definitionIndex++)
            {
                HeroDefinition definition = _clearedDefinitions[definitionIndex];
                if (definition != null)
                {
                    SetClearedField(definition, _clearedPrefabs[definitionIndex]);
                }
            }
            _clearedDefinitions.Clear();
            _clearedPrefabs.Clear();
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// 이번 빌드에서 비우는 쪽 칸(webPrefab 또는 desktopPrefab)에 값을 넣고 저장 대상으로 표시한다.
        /// </summary>
        private static void SetClearedField(HeroDefinition definition, GameObject prefab)
        {
            if (_hasClearedWebPrefab)
            {
                definition.webPrefab = prefab;
            }
            else
            {
                definition.desktopPrefab = prefab;
            }
            EditorUtility.SetDirty(definition);
        }
    }
}
