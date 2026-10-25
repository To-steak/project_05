using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project05.UI.View
{
    /// <summary>
    /// [Main] 메인 메뉴. 게임 시작·설정 버튼과 최고 점수·최고 도달 웨이브 텍스트. 표시만 담당하며 World와 App을 모른다.
    /// </summary>
    public class MainMenuView : MonoBehaviour
    {
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private TMP_Text _waveText;
        [SerializeField] private TMP_Text _scoreText;

        /// <summary>
        /// 게임 시작 버튼. Presenter가 onClick을 구독한다.
        /// </summary>
        public Button StartButton
        {
            get { return _startButton; }
        }

        /// <summary>
        /// 설정 버튼. Presenter가 onClick을 구독한다.
        /// </summary>
        public Button SettingsButton
        {
            get { return _settingsButton; }
        }

        /// <summary>
        /// 최고 점수 텍스트를 바꾼다.
        /// </summary>
        /// <param name="text">표시할 문장 (Presenter가 만든다)</param>
        public void SetScoreText(string text)
        {
            if (_scoreText != null)
            {
                _scoreText.text = text;
            }
        }

        /// <summary>
        /// 최고 웨이브 텍스트를 바꾼다.
        /// </summary>
        /// <param name="text">표시할 문장 (Presenter가 만든다)</param>
        public void SetWaveText(string text)
        {
            if (_waveText != null)
            {
                _waveText.text = text;
            }
        }
    }
}
