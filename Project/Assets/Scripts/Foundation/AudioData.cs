using System;
using UnityEngine;

namespace Project05.Foundation
{
    /// <summary>
    /// 오디오 테이블. AudioDefinition을 게임 시작 시 DefinitionBaker.BakeAudio가 채우고 AudioPlayer가 읽는다.
    /// 클립 배열은 종류 enum을 인덱스로 쓰며 길이가 항상 enum 항목 수와 같다. 비어 있는 칸(null)은 소리를 내지 않는다.
    /// </summary>
    public class AudioData
    {
        // 배경음 종류별 클립 (인덱스 = BackgroundMusicKind, None 칸은 항상 null)
        public readonly AudioClip[] musicClips = new AudioClip[Enum.GetValues(typeof(BackgroundMusicKind)).Length];
        // 효과음 종류별 클립 (인덱스 = SoundEffectKind)
        public readonly AudioClip[] soundEffectClips = new AudioClip[Enum.GetValues(typeof(SoundEffectKind)).Length];
        // 배경음 믹스 배율 (0 ~ 1, 설정의 BGM 볼륨에 곱한다)
        public float musicVolumeScale;
        // 효과음 믹스 배율 (0 ~ 1, 설정의 SFX 볼륨에 곱한다)
        public float soundEffectVolumeScale;
        // 효과음을 동시에 낼 수 있는 수 (= 효과음 AudioSource 수). 정의가 없어도 AudioSource가 하나는 생기도록 1에서 시작한다
        public int soundEffectVoiceCount = 1;
    }
}
