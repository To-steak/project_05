using UnityEngine;
using UnityEngine.UI;

namespace Project05.UI.View
{
    /// <summary>
    /// [Main·Game] 설정 패널. BGM·SFX 볼륨 슬라이더(0 ~ 1)와 닫기 버튼. 표시만 담당하며 World와 App을 모른다.
    /// 이 컴포넌트는 항상 켜진 오브젝트에 두고, 실제 패널은 _panelRoot로 켜고 끈다.
    /// </summary>
    public class SettingsView : MonoBehaviour
    {
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private Slider _bgmVolumeSlider;
        [SerializeField] private Slider _sfxVolumeSlider;
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
        /// 슬라이더 값을 이벤트 없이 맞춘다 (패널을 열 때 저장된 값으로).
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
        }
    }
}
