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
#if UNITY_WEBGL && !UNITY_EDITOR
        // WebGL 빌드는 브라우저 정책상 첫 사용자 입력 전까지 소리를 낼 수 없다 (DESIGN 3.7, 6장)
        private const bool IS_AUDIO_LOCKED_UNTIL_INPUT = true;
#else
        private const bool IS_AUDIO_LOCKED_UNTIL_INPUT = false;
#endif

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
        /// 오디오용 AudioSource는 Runner 오브젝트에 붙인다.
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
            // 볼륨을 읽으므로 Settings 다음에 만든다
            App.Audio = CreateAudioPlayer(runnerObject, DefinitionBaker.BakeAudio(definition));
        }

        /// <summary>
        /// Runner 오브젝트에 배경음 1개 + 효과음 동시 재생 수만큼 AudioSource를 붙여 AudioPlayer를 만든다 (DESIGN 5.3).
        /// </summary>
        private static AudioPlayer CreateAudioPlayer(GameObject runnerObject, AudioData audioData)
        {
            AudioSource musicSource = runnerObject.AddComponent<AudioSource>();
            AudioSource[] soundEffectSources = new AudioSource[audioData.soundEffectVoiceCount];
            for (int sourceIndex = 0; sourceIndex < soundEffectSources.Length; sourceIndex++)
            {
                soundEffectSources[sourceIndex] = runnerObject.AddComponent<AudioSource>();
            }
            return new AudioPlayer(musicSource, soundEffectSources, audioData, App.Settings, !IS_AUDIO_LOCKED_UNTIL_INPUT);
        }
    }
}
