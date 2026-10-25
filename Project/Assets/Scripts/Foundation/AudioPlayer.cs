using UnityEngine;

namespace Project05.Foundation
{
    /// <summary>
    /// 배경음·효과음 재생 (App.Audio). AudioSource는 Bootstrap이 Runner 오브젝트에 붙여서 넘긴다 (DESIGN 5.3).
    /// 볼륨은 Runner가 매 프레임 SynchronizeVolumes로 App.Settings 값을 반영한다 (설정 슬라이더가 즉시 들린다).
    /// WebGL에서는 첫 사용자 입력 전까지 잠겨 있다 (DESIGN 3.7, 6장). 잠긴 동안 효과음은 버리고, 배경음은 요청만 기억했다가 Unlock에서 튼다.
    /// 같은 효과음은 한 프레임에 한 번만 낸다 (적 수십 마리가 한 프레임에 맞아도 한 번). 동시에 낼 수 있는 수는 효과음 AudioSource 수다.
    /// </summary>
    public class AudioPlayer
    {
        private readonly AudioSource _musicSource;
        private readonly AudioSource[] _soundEffectSources;
        private readonly AudioData _data;
        private readonly SettingsStore _settings;
        // 효과음 종류별 마지막으로 낸 Time.frameCount (인덱스 = SoundEffectKind)
        private readonly int[] _lastPlayedFrameBySoundEffect;
        // 다음 효과음을 먼저 찾아볼 AudioSource 인덱스 (돌아가며 쓴다)
        private int _nextSoundEffectSourceIndex;
        private bool _isUnlocked;
        // 마지막으로 요청받은 배경음 (잠긴 동안에도 기억한다)
        private BackgroundMusicKind _requestedMusicKind;
        // 지금 틀고 있는 배경음
        private BackgroundMusicKind _playingMusicKind;
        // 마지막으로 AudioSource에 넣은 볼륨 (바뀔 때만 다시 넣는다)
        private float _appliedMusicVolume = NOT_APPLIED_VOLUME;
        private float _appliedSoundEffectVolume = NOT_APPLIED_VOLUME;

        // 아직 볼륨을 넣지 않았다는 표시 (0 ~ 1 밖의 값)
        private const float NOT_APPLIED_VOLUME = -1f;
        // 효과음을 아직 낸 적 없는 프레임 표시
        private const int NEVER_PLAYED_FRAME = -1;

        /// <summary>
        /// 첫 사용자 입력으로 잠금이 풀렸는지. WebGL이 아니면 처음부터 풀려 있다.
        /// </summary>
        public bool IsUnlocked
        {
            get { return _isUnlocked; }
        }

        /// <summary>
        /// AudioSource를 2D 재생용으로 설정하고 현재 설정 볼륨을 반영한다.
        /// </summary>
        /// <param name="musicSource">배경음용 AudioSource (반복 재생)</param>
        /// <param name="soundEffectSources">효과음용 AudioSource 묶음 (길이 = 동시 재생 수, 1 이상)</param>
        /// <param name="data">베이킹된 오디오 테이블</param>
        /// <param name="settings">볼륨을 읽을 설정</param>
        /// <param name="isUnlocked">처음부터 재생할 수 있는지 (WebGL 빌드에서는 false)</param>
        public AudioPlayer(AudioSource musicSource, AudioSource[] soundEffectSources, AudioData data, SettingsStore settings, bool isUnlocked)
        {
            _musicSource = musicSource;
            _soundEffectSources = soundEffectSources;
            _data = data;
            _settings = settings;
            _isUnlocked = isUnlocked;

            ConfigureSource(_musicSource);
            _musicSource.loop = true;
            for (int sourceIndex = 0; sourceIndex < _soundEffectSources.Length; sourceIndex++)
            {
                ConfigureSource(_soundEffectSources[sourceIndex]);
            }

            _lastPlayedFrameBySoundEffect = new int[_data.soundEffectClips.Length];
            for (int kindIndex = 0; kindIndex < _lastPlayedFrameBySoundEffect.Length; kindIndex++)
            {
                _lastPlayedFrameBySoundEffect[kindIndex] = NEVER_PLAYED_FRAME;
            }

            SynchronizeVolumes();
        }

        /// <summary>
        /// 재생 잠금을 푼다. Runner가 첫 사용자 입력(클릭·키)을 본 프레임에 호출한다. 잠긴 동안 요청된 배경음을 바로 튼다.
        /// </summary>
        public void Unlock()
        {
            if (_isUnlocked)
            {
                return;
            }

            _isUnlocked = true;
            StartRequestedMusic();
        }

        /// <summary>
        /// 배경음을 바꾼다. 이미 같은 배경음이면 처음부터 다시 틀지 않으므로 매 프레임 호출해도 된다.
        /// 잠긴 동안에는 요청만 기억한다.
        /// </summary>
        /// <param name="musicKind">틀 배경음 (None이면 정지)</param>
        public void PlayBackgroundMusic(BackgroundMusicKind musicKind)
        {
            _requestedMusicKind = musicKind;
            if (!_isUnlocked)
            {
                return;
            }

            StartRequestedMusic();
        }

        /// <summary>
        /// 효과음을 한 번 낸다. 잠겨 있거나, 클립이 비어 있거나, 이번 프레임에 이미 낸 종류면 무시한다.
        /// 쉬고 있는 AudioSource를 먼저 쓰고, 모두 재생 중이면 가장 오래전에 고른 것을 끊고 쓴다.
        /// </summary>
        /// <param name="soundEffectKind">낼 효과음 종류</param>
        public void PlaySoundEffect(SoundEffectKind soundEffectKind)
        {
            int kindIndex = (int)soundEffectKind;
            if (!_isUnlocked || kindIndex < 0 || kindIndex >= _data.soundEffectClips.Length)
            {
                return;
            }

            AudioClip clip = _data.soundEffectClips[kindIndex];
            int frame = Time.frameCount;
            if (clip == null || _lastPlayedFrameBySoundEffect[kindIndex] == frame)
            {
                return;
            }
            _lastPlayedFrameBySoundEffect[kindIndex] = frame;

            AudioSource source = PickSoundEffectSource();
            source.clip = clip;
            source.Play();
        }

        /// <summary>
        /// 설정 볼륨 × 믹스 배율을 AudioSource에 반영한다. 값이 바뀌었을 때만 넣는다. Runner가 매 프레임 호출한다.
        /// </summary>
        public void SynchronizeVolumes()
        {
            float musicVolume = _settings.BGMVolume * _data.musicVolumeScale;
            if (musicVolume != _appliedMusicVolume)
            {
                _appliedMusicVolume = musicVolume;
                _musicSource.volume = musicVolume;
            }

            float soundEffectVolume = _settings.SFXVolume * _data.soundEffectVolumeScale;
            if (soundEffectVolume != _appliedSoundEffectVolume)
            {
                _appliedSoundEffectVolume = soundEffectVolume;
                for (int sourceIndex = 0; sourceIndex < _soundEffectSources.Length; sourceIndex++)
                {
                    _soundEffectSources[sourceIndex].volume = soundEffectVolume;
                }
            }
        }

        private void StartRequestedMusic()
        {
            if (_requestedMusicKind == _playingMusicKind)
            {
                return;
            }
            _playingMusicKind = _requestedMusicKind;

            AudioClip clip = _data.musicClips[(int)_playingMusicKind];
            if (clip == null)
            {
                _musicSource.Stop();
                _musicSource.clip = null;
                return;
            }

            // 종류가 달라도 같은 클립이면(보스 배경음이 비어 일반 배경음으로 채워진 경우) 끊지 않고 이어 튼다
            if (_musicSource.clip == clip && _musicSource.isPlaying)
            {
                return;
            }

            _musicSource.clip = clip;
            _musicSource.Play();
        }

        private AudioSource PickSoundEffectSource()
        {
            int sourceCount = _soundEffectSources.Length;
            for (int offset = 0; offset < sourceCount; offset++)
            {
                int sourceIndex = (_nextSoundEffectSourceIndex + offset) % sourceCount;
                if (!_soundEffectSources[sourceIndex].isPlaying)
                {
                    _nextSoundEffectSourceIndex = (sourceIndex + 1) % sourceCount;
                    return _soundEffectSources[sourceIndex];
                }
            }

            AudioSource oldestSource = _soundEffectSources[_nextSoundEffectSourceIndex];
            _nextSoundEffectSourceIndex = (_nextSoundEffectSourceIndex + 1) % sourceCount;
            return oldestSource;
        }

        private static void ConfigureSource(AudioSource source)
        {
            source.playOnAwake = false;
            source.loop = false;
            // 화면 위치와 관계없이 같은 크기로 들리는 2D 소리
            source.spatialBlend = 0f;
        }
    }
}
