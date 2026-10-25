using UnityEngine;

namespace Project05.Authoring.Definition
{
    /// <summary>
    /// 배경음·효과음 클립과 믹스 수치. GameDefinition.audio에 연결하면 게임 시작 시 DefinitionBaker가 AudioData로 베이킹한다.
    /// 비워 둔 클립은 소리를 내지 않는다. 효과음 하나는 대부분 프레임 기록 하나에 대응한다 (DESIGN 5.7).
    /// </summary>
    [CreateAssetMenu(fileName = "AudioDefinition", menuName = "Project05/Definition/Audio")]
    public class AudioDefinition : ScriptableObject
    {
        // ───── 배경음 (반복 재생) ─────

        // Main 씬 메뉴 배경음
        public AudioClip mainMenuMusic;
        // Game 씬 일반 진행 배경음
        public AudioClip gameMusic;
        // 보스가 살아 있는 동안의 배경음 (비우면 일반 배경음을 이어 튼다)
        public AudioClip bossMusic;

        // ───── 믹스 ─────

        // 배경음 믹스 배율 (설정의 BGM 볼륨에 곱한다)
        [Range(0f, 1f)] public float musicVolumeScale = 1f;
        // 효과음 믹스 배율 (설정의 SFX 볼륨에 곱한다)
        [Range(0f, 1f)] public float soundEffectVolumeScale = 1f;
        // 효과음 동시 재생 수 (= 효과음 AudioSource 수. 넘치면 가장 오래된 소리를 끊는다)
        [Min(1)] public int soundEffectVoiceCount = 16;

        // ───── 효과음: 흐름 ─────

        // 웨이브 시작
        public AudioClip waveStartedClip;
        // 웨이브 클리어
        public AudioClip waveClearedClip;
        // 보스 등장 경고
        public AudioClip bossSpawnedClip;
        // 스테이지 클리어
        public AudioClip stageClearedClip;
        // 레벨업 (선택지가 뜰 때)
        public AudioClip levelUpClip;
        // 패배
        public AudioClip defeatClip;

        // ───── 효과음: 히어로 ─────

        // 근거리 광역 공격
        public AudioClip heroMeleeAttackClip;
        // 원거리 발사
        public AudioClip heroRangedShotClip;
        // 피격
        public AudioClip heroDamagedClip;
        // 사망
        public AudioClip heroDiedClip;
        // 부활
        public AudioClip heroRespawnedClip;

        // ───── 효과음: 타워 ─────

        // 건설
        public AudioClip towerBuiltClip;
        // 강화
        public AudioClip towerUpgradedClip;
        // 자동 사격
        public AudioClip towerShotClip;
        // 탑승 중 클릭 사격 (비우면 자동 사격 소리를 쓴다)
        public AudioClip towerBoardedShotClip;
        // 피격
        public AudioClip towerDamagedClip;
        // 파괴
        public AudioClip towerDestroyedClip;
        // 수리
        public AudioClip towerRepairedClip;

        // ───── 효과음: 적 ─────

        // 피격
        public AudioClip enemyDamagedClip;
        // 일반 적 사망
        public AudioClip enemyDiedClip;
        // 보스 사망 (비우면 일반 적 사망 소리를 쓴다)
        public AudioClip bossDiedClip;
        // 코어 공격
        public AudioClip enemyCoreAttackClip;
        // 타워 공격
        public AudioClip enemyTowerAttackClip;

        // ───── 효과음: 드롭·코어·명령 ─────

        // 골드 획득
        public AudioClip goldCollectedClip;
        // XP 획득
        public AudioClip XPCollectedClip;
        // 코어 피격
        public AudioClip coreDamagedClip;
        // 명령 거부 (골드 부족, 사거리 밖 등)
        public AudioClip commandRejectedClip;
    }
}
