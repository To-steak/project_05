using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project05.Foundation
{
    /// <summary>
    /// 씬 로드 (DESIGN 3.7).
    /// - Main → Game: 로딩 패널이 BeginLoadGame으로 비동기 로드를 시작하고(allowSceneActivation = false),
    ///   진행률 0.9에서 로드 완료로 본다. 시작 버튼을 누르면 ActivateLoadedGame으로 씬을 활성화한다.
    /// - 결과 패널의 재시작·메인으로는 동기 로드(LoadGame, LoadMain)를 쓴다.
    /// </summary>
    public class SceneLoader
    {
        // Main 씬 이름 (Build Settings와 같아야 한다)
        public const string MAIN_SCENE_NAME = "Main";
        // Game 씬 이름 (Build Settings와 같아야 한다)
        public const string GAME_SCENE_NAME = "Game";

        private AsyncOperation _loadOperation;

        // allowSceneActivation = false인 비동기 로드는 progress가 이 값에서 멈춘다 (나머지는 활성화 단계)
        private const float LOAD_READY_PROGRESS = 0.9f;

        /// <summary>
        /// Game 씬 비동기 로드가 진행 중인지 (시작 후 활성화 전까지)
        /// </summary>
        public bool IsLoading
        {
            get { return _loadOperation != null; }
        }

        /// <summary>
        /// 로드 진행률 (0 ~ 1). 0.9에서 멈추는 Unity 진행률을 0 ~ 1로 바꾼 값이다. 로드 중이 아니면 0.
        /// </summary>
        public float LoadProgress
        {
            get { return _loadOperation == null ? 0f : Mathf.Clamp01(_loadOperation.progress / LOAD_READY_PROGRESS); }
        }

        /// <summary>
        /// 로드가 끝나 활성화만 남았는지 (진행률 0.9 도달)
        /// </summary>
        public bool IsLoadReady
        {
            get { return _loadOperation != null && _loadOperation.progress >= LOAD_READY_PROGRESS; }
        }

        /// <summary>
        /// Game 씬 비동기 로드를 시작한다. 로드가 끝나도 ActivateLoadedGame 전까지 활성화하지 않는다. 이미 로드 중이면 무시한다.
        /// </summary>
        public void BeginLoadGame()
        {
            if (_loadOperation != null)
            {
                return;
            }

            _loadOperation = SceneManager.LoadSceneAsync(GAME_SCENE_NAME);
            if (_loadOperation != null)
            {
                _loadOperation.allowSceneActivation = false;
            }
        }

        /// <summary>
        /// 로드가 끝난 Game 씬을 활성화한다. 아직 준비되지 않았으면 무시한다.
        /// 이 호출(시작 버튼 클릭)이 WebGL 오디오 잠금 해제용 사용자 입력도 겸한다.
        /// </summary>
        public void ActivateLoadedGame()
        {
            if (!IsLoadReady)
            {
                return;
            }

            _loadOperation.allowSceneActivation = true;
            _loadOperation = null;
        }

        /// <summary>
        /// Game 씬을 처음부터 다시 로드한다 (결과 패널의 재시작). 이미 Game 씬에 있어도 새로 로드되어 Runner가 새 판을 시작한다.
        /// </summary>
        public void LoadGame()
        {
            SceneManager.LoadScene(GAME_SCENE_NAME);
        }

        /// <summary>
        /// Main 씬을 로드한다 (결과 패널의 메인으로).
        /// </summary>
        public void LoadMain()
        {
            SceneManager.LoadScene(MAIN_SCENE_NAME);
        }
    }
}
