using Project05.Foundation;
using Project05.UI.View;

namespace Project05.UI.Presenter
{
    /// <summary>
    /// [Main] 설정 패널. 열 때 App.Settings의 값을 슬라이더에 맞추고, 슬라이더가 바뀌면 App.Settings에 쓴다.
    /// 닫을 때 한 번 디스크에 기록한다 (WebGL에서 슬라이더를 움직일 때마다 IndexedDB에 쓰지 않도록).
    /// </summary>
    public class SettingsPresenter
    {
        private readonly SettingsView _view;

        /// <summary>
        /// 설정 패널 View를 받아 슬라이더·닫기 버튼을 연결하고 패널을 숨긴다.
        /// </summary>
        /// <param name="view">씬의 설정 패널 View</param>
        public SettingsPresenter(SettingsView view)
        {
            _view = view;
            _view.SetVisible(false);
            if (_view.BGMVolumeSlider != null)
            {
                _view.BGMVolumeSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
            }
            if (_view.SFXVolumeSlider != null)
            {
                _view.SFXVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            }
            if (_view.CloseButton != null)
            {
                _view.CloseButton.onClick.AddListener(Close);
            }
        }

        /// <summary>
        /// 저장된 값으로 슬라이더를 맞추고 패널을 연다.
        /// </summary>
        public void Open()
        {
            _view.SetVolumesWithoutNotify(App.Settings.BGMVolume, App.Settings.SFXVolume);
            _view.SetVisible(true);
        }

        private void Close()
        {
            App.Settings.Save();
            _view.SetVisible(false);
        }

        private void OnBGMVolumeChanged(float volume)
        {
            App.Settings.SetBGMVolume(volume);
        }

        private void OnSFXVolumeChanged(float volume)
        {
            App.Settings.SetSFXVolume(volume);
        }
    }
}
