using UnityEngine;

namespace Project05.Foundation
{
    /// <summary>
    /// 설정을 PlayerPrefs에 저장·로드한다 (WebGL에서는 IndexedDB에 저장된다, DESIGN 3.7).
    /// 지금 항목은 BGM·SFX 볼륨(0 ~ 1)뿐이다. AudioPlayer가 매 프레임 이 값을 읽어 소리에 반영한다.
    /// 값은 바꿀 때 메모리와 PlayerPrefs에 쓰고, 디스크 기록(PlayerPrefs.Save)은 Save에서 한 번만 한다.
    /// </summary>
    public class SettingsStore
    {
        private float _bgmVolume;
        private float _sfxVolume;

        private const string BGM_VOLUME_KEY = "Settings.BGMVolume";
        private const string SFX_VOLUME_KEY = "Settings.SFXVolume";
        // 저장된 값이 없을 때의 볼륨 (최대)
        private const float DEFAULT_VOLUME = 1f;

        /// <summary>
        /// BGM 볼륨 (0 ~ 1)
        /// </summary>
        public float BGMVolume
        {
            get { return _bgmVolume; }
        }

        /// <summary>
        /// SFX 볼륨 (0 ~ 1)
        /// </summary>
        public float SFXVolume
        {
            get { return _sfxVolume; }
        }

        /// <summary>
        /// PlayerPrefs에서 설정을 읽어 둔다. 없으면 기본값을 쓴다.
        /// </summary>
        public SettingsStore()
        {
            _bgmVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(BGM_VOLUME_KEY, DEFAULT_VOLUME));
            _sfxVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(SFX_VOLUME_KEY, DEFAULT_VOLUME));
        }

        /// <summary>
        /// BGM 볼륨을 바꾼다. 디스크 기록은 Save에서 한다.
        /// </summary>
        /// <param name="volume">새 볼륨 (0 ~ 1로 자른다)</param>
        public void SetBGMVolume(float volume)
        {
            _bgmVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat(BGM_VOLUME_KEY, _bgmVolume);
        }

        /// <summary>
        /// SFX 볼륨을 바꾼다. 디스크 기록은 Save에서 한다.
        /// </summary>
        /// <param name="volume">새 볼륨 (0 ~ 1로 자른다)</param>
        public void SetSFXVolume(float volume)
        {
            _sfxVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat(SFX_VOLUME_KEY, _sfxVolume);
        }

        /// <summary>
        /// 바뀐 설정을 디스크에 기록한다. 설정 패널을 닫을 때 호출한다 (WebGL에서 매번 기록하지 않도록).
        /// </summary>
        public void Save()
        {
            PlayerPrefs.Save();
        }
    }
}
