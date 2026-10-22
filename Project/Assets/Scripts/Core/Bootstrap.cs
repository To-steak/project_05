using Project05.Authoring.Definition;
using Project05.Core.Baking;
using Project05.Foundation;
using UnityEngine;

namespace Project05.Core
{
    /// <summary>
    /// 게임 시작 시 1회 실행되는 부트스트랩. 씬에 아무것도 배치하지 않아도 Runner가 생긴다.
    /// </summary>
    public static class Bootstrap
    {
        private const string RUNNER_OBJECT_NAME = "Runner";

        /// <summary>
        /// static 값을 초기화한다. 도메인 리로드를 끈 Play에서도 이전 Play의 값이 남지 않게 한다.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void ResetStatics()
        {
            App.Reset();
        }

        /// <summary>
        /// 첫 씬 로드 전에 DDOL Runner를 만들고 App을 채운다. 정의(~Definition) 베이킹도 여기서 1회 한다.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            GameObject runnerObject = new GameObject(RUNNER_OBJECT_NAME);
            Object.DontDestroyOnLoad(runnerObject);
            runnerObject.AddComponent<Runner>();
            GameDefinition definition = DefinitionBaker.Load();
            App.Data = DefinitionBaker.Bake(definition);
            App.ViewData = DefinitionBaker.BakeView(definition);
            App.Pool = new ViewPool();
            App.Scene = new SceneLoader();
            App.Settings = new SettingsStore();
            App.Records = new RecordStore();
        }
    }
}
