using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project05.UI.View
{
    /// <summary>
    /// [Main·Game] 설정 패널. BGM·SFX 볼륨 슬라이더(0 ~ 1)와 현재 값 글자(0 ~ 100 정수), 닫기 버튼. 표시만 담당하며 World와 App을 모른다.
    /// 이 컴포넌트는 항상 켜진 오브젝트에 두고, 실제 패널은 _panelRoot로 켜고 끈다.
    /// </summary>
    public class SettingsView : MonoBehaviour
    {
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private Slider _bgmVolumeSlider;
        [SerializeField] private Slider _sfxVolumeSlider;
        // BGM 볼륨 현재 값 글자 (비우면 표시하지 않는다)
        [SerializeField] private TMP_Text _bgmVolumeText;
        // SFX 볼륨 현재 값 글자 (비우면 표시하지 않는다)
        [SerializeField] private TMP_Text _sfxVolumeText;
        [SerializeField] private Button _closeButton;

        /// <summary>
        /// BGM 볼륨 슬라이더. Presenter가 onValueChanged를 구독한다.
        /// </summary>
        public Slider BGMVolumeSlider
        {
            get { return _bgmVolumeSlider; }
        }

        /// <summary>
        /// SFX 볼륨 슬라이더. Presenter가 onValueChanged를 구독한다.
        /// </summary>
        public Slider SFXVolumeSlider
        {
            get { return _sfxVolumeSlider; }
        }

        /// <summary>
        /// 닫기 버튼. Presenter가 onClick을 구독한다.
        /// </summary>
        public Button CloseButton
        {
            get { return _closeButton; }
        }

        /// <summary>
        /// 패널을 보이거나 숨긴다.
        /// </summary>
        /// <param name="isVisible">보이면 true</param>
        public void SetVisible(bool isVisible)
        {
            if (_panelRoot != null && _panelRoot.activeSelf != isVisible)
            {
                _panelRoot.SetActive(isVisible);
            }
        }

        /// <summary>
        /// 슬라이더 값을 이벤트 없이 맞추고 현재 값 글자도 맞춘다 (패널을 열 때 저장된 값으로).
        /// </summary>
        /// <param name="bgmVolume">BGM 볼륨 (0 ~ 1)</param>
        /// <param name="sfxVolume">SFX 볼륨 (0 ~ 1)</param>
        public void SetVolumesWithoutNotify(float bgmVolume, float sfxVolume)
        {
            if (_bgmVolumeSlider != null)
            {
                _bgmVolumeSlider.SetValueWithoutNotify(bgmVolume);
            }
            if (_sfxVolumeSlider != null)
            {
                _sfxVolumeSlider.SetValueWithoutNotify(sfxVolume);
            }
            SetBGMVolumeText(bgmVolume);
            SetSFXVolumeText(sfxVolume);
        }

        /// <summary>
        /// BGM 볼륨 현재 값을 0 ~ 100 정수로 보여 준다.
        /// </summary>
        /// <param name="volume">BGM 볼륨 (0 ~ 1)</param>
        public void SetBGMVolumeText(float volume)
        {
            SetVolumeText(_bgmVolumeText, volume);
        }

        /// <summary>
        /// SFX 볼륨 현재 값을 0 ~ 100 정수로 보여 준다.
        /// </summary>
        /// <param name="volume">SFX 볼륨 (0 ~ 1)</param>
        public void SetSFXVolumeText(float volume)
        {
            SetVolumeText(_sfxVolumeText, volume);
        }

        /// <summary>
        /// 볼륨(0 ~ 1)을 반올림한 정수 퍼센트로 쓴다. 슬라이더를 움직일 때만 불리므로 매 프레임 할당은 아니다.
        /// </summary>
        private static void SetVolumeText(TMP_Text volumeText, float volume)
        {
            if (volumeText == null)
            {
                return;
            }
            volumeText.text = Mathf.RoundToInt(Mathf.Clamp01(volume) * VOLUME_DISPLAY_MAX).ToString();
        }

        // 볼륨 글자의 최댓값 (볼륨 1 = 100)
        private const int VOLUME_DISPLAY_MAX = 100;
    }
}
