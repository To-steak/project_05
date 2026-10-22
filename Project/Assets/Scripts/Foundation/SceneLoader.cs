using UnityEngine.SceneManagement;

namespace Project05.Foundation
{
    /// <summary>
    /// 씬 로드. 지금은 결과 패널의 재시작·메인으로 버튼이 쓰는 동기 로드만 있다.
    /// 비동기 로드와 allowSceneActivation 제어(로딩 패널)는 feat/main-scene에서 추가한다 (DESIGN 3.7).
    /// </summary>
    public class SceneLoader
    {
        // Main 씬 이름 (Build Settings와 같아야 한다)
        public const string MAIN_SCENE_NAME = "Main";
        // Game 씬 이름 (Build Settings와 같아야 한다)
        public const string GAME_SCENE_NAME = "Game";

        /// <summary>
        /// Game 씬을 처음부터 다시 로드한다. 이미 Game 씬에 있어도 새로 로드되어 Runner가 새 판을 시작한다.
        /// </summary>
        public void LoadGame()
        {
            SceneManager.LoadScene(GAME_SCENE_NAME);
        }

        /// <summary>
        /// Main 씬을 로드한다.
        /// </summary>
        public void LoadMain()
        {
            SceneManager.LoadScene(MAIN_SCENE_NAME);
        }
    }
}
