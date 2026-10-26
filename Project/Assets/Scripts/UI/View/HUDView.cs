using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project05.UI.View
{
    /// <summary>
    /// [Game] 인게임 HUD. 스테이지-웨이브, 처치 수, 준비 시간·보스 안내, 히어로 HP·목숨·부활, 코어 HP, 골드, 레벨·XP, 점수.
    /// 표시만 담당하며 World와 App을 모른다. Presenter가 값이 바뀐 항목만 호출한다.
    /// 바는 Slider로 표시한다 (Interactable을 끄고 Handle을 지워 표시 전용으로 쓴다). 모든 참조는 선택이다. 연결하지 않은 항목은 건너뛴다 (배치는 에디터에서 자유롭게 한다).
    /// 숫자는 TMP_Text.SetText(format, 숫자)로 넣어 문자열을 만들지 않는다 (매 프레임 할당 방지).
    /// </summary>
    public class HUDView : MonoBehaviour
    {
        [Header("웨이브")]
        [SerializeField] private TMP_Text _waveText;
        [SerializeField] private TMP_Text _killProgressText;
        [SerializeField] private Slider _killProgressSlider;
        // 준비 시간·보스 안내를 담는 오브젝트 (없으면 텍스트만 끄고 켠다)
        [SerializeField] private GameObject _phaseStatusRoot;
        [SerializeField] private TMP_Text _phaseStatusText;

        [Header("히어로")]
        [SerializeField] private TMP_Text _heroHPText;
        [SerializeField] private Slider _heroHPSlider;
        [SerializeField] private TMP_Text _livesText;
        // 부활 대기 안내를 담는 오브젝트 (없으면 텍스트만 끄고 켠다)
        [SerializeField] private GameObject _respawnRoot;
        [SerializeField] private TMP_Text _respawnText;

        [Header("코어")]
        [SerializeField] private TMP_Text _coreHPText;
        [SerializeField] private Slider _coreHPSlider;

        [Header("성장")]
        [SerializeField] private TMP_Text _goldText;
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private TMP_Text _levelXPText;
        [SerializeField] private Slider _levelXPSlider;
        [SerializeField] private TMP_Text _scoreText;

        private const string WAVE_FORMAT = "{0} - {1}";
        private const string RATIO_FORMAT = "{0} / {1}";
        private const string PREPARATION_FORMAT = "다음 웨이브까지 {0}초";
        private const string BOSS_TEXT = "보스 등장";
        private const string LIVES_FORMAT = "남은 목숨 {0}개";
        private const string RESPAWN_FORMAT = "부활까지 {0}초";
        private const string GOLD_FORMAT = "획득한 골드 {0}G";
        private const string LEVEL_FORMAT = "레벨 {0}";
        private const string SCORE_FORMAT = "점수 {0}";

        /// <summary>
        /// 스테이지-웨이브 이름을 표시한다 (예: 1-3).
        /// </summary>
        /// <param name="stageNumber">스테이지 번호 n</param>
        /// <param name="waveNumber">웨이브 번호 x</param>
        public void SetWave(int stageNumber, int waveNumber)
        {
            if (_waveText != null)
            {
                _waveText.SetText(WAVE_FORMAT, stageNumber, waveNumber);
            }
        }

        /// <summary>
        /// 이번 웨이브 처치 진행도를 표시한다 (current/max).
        /// </summary>
        /// <param name="killCount">처치 수</param>
        /// <param name="waveEnemyCount">이번 웨이브 적 수</param>
        public void SetKillProgress(int killCount, int waveEnemyCount)
        {
            if (_killProgressText != null)
            {
                _killProgressText.SetText(RATIO_FORMAT, killCount, waveEnemyCount);
            }
            SetSliderRatio(_killProgressSlider, killCount, waveEnemyCount);
        }

        /// <summary>
        /// 웨이브 사이 준비 시간 안내를 보인다.
        /// </summary>
        /// <param name="remainingSeconds">다음 웨이브까지 남은 초 (올림)</param>
        public void ShowPreparation(int remainingSeconds)
        {
            SetPhaseStatusVisible(true);
            if (_phaseStatusText != null)
            {
                _phaseStatusText.SetText(PREPARATION_FORMAT, remainingSeconds);
            }
        }

        /// <summary>
        /// 보스 등장 안내를 보인다.
        /// </summary>
        public void ShowBoss()
        {
            SetPhaseStatusVisible(true);
            if (_phaseStatusText != null)
            {
                _phaseStatusText.SetText(BOSS_TEXT);
            }
        }

        /// <summary>
        /// 준비 시간·보스 안내를 숨긴다.
        /// </summary>
        public void HidePhaseStatus()
        {
            SetPhaseStatusVisible(false);
        }

        /// <summary>
        /// 히어로 HP를 표시한다.
        /// </summary>
        /// <param name="currentHP">현재 HP</param>
        /// <param name="maxHP">최대 HP</param>
        public void SetHeroHP(int currentHP, int maxHP)
        {
            if (_heroHPText != null)
            {
                _heroHPText.SetText(RATIO_FORMAT, currentHP, maxHP);
            }
            SetSliderRatio(_heroHPSlider, currentHP, maxHP);
        }

        /// <summary>
        /// 남은 목숨을 표시한다.
        /// </summary>
        /// <param name="remainingLives">남은 목숨</param>
        public void SetLives(int remainingLives)
        {
            if (_livesText != null)
            {
                _livesText.SetText(LIVES_FORMAT, remainingLives);
            }
        }

        /// <summary>
        /// 부활 대기 안내를 보인다.
        /// </summary>
        /// <param name="remainingSeconds">부활까지 남은 초 (올림)</param>
        public void ShowRespawn(int remainingSeconds)
        {
            SetRespawnVisible(true);
            if (_respawnText != null)
            {
                _respawnText.SetText(RESPAWN_FORMAT, remainingSeconds);
            }
        }

        /// <summary>
        /// 부활 대기 안내를 숨긴다.
        /// </summary>
        public void HideRespawn()
        {
            SetRespawnVisible(false);
        }

        /// <summary>
        /// 코어 HP를 표시한다.
        /// </summary>
        /// <param name="currentHP">현재 HP</param>
        /// <param name="maxHP">최대 HP</param>
        public void SetCoreHP(int currentHP, int maxHP)
        {
            if (_coreHPText != null)
            {
                _coreHPText.SetText(RATIO_FORMAT, currentHP, maxHP);
            }
            SetSliderRatio(_coreHPSlider, currentHP, maxHP);
        }

        /// <summary>
        /// 보유 골드를 표시한다.
        /// </summary>
        /// <param name="gold">보유 골드</param>
        public void SetGold(int gold)
        {
            if (_goldText != null)
            {
                _goldText.SetText(GOLD_FORMAT, gold);
            }
        }

        /// <summary>
        /// 히어로 레벨과 현재 레벨에서 모은 XP를 표시한다.
        /// </summary>
        /// <param name="level">레벨 (1부터)</param>
        /// <param name="currentXP">현재 레벨에서 모은 XP</param>
        /// <param name="requiredXP">다음 레벨까지 필요한 XP (0이면 레벨업 없음)</param>
        public void SetLevel(int level, int currentXP, int requiredXP)
        {
            if (_levelText != null)
            {
                _levelText.SetText(LEVEL_FORMAT, level);
            }
            if (_levelXPText != null)
            {
                _levelXPText.SetText(RATIO_FORMAT, currentXP, requiredXP);
            }
            SetSliderRatio(_levelXPSlider, currentXP, requiredXP);
        }

        /// <summary>
        /// 점수를 표시한다.
        /// </summary>
        /// <param name="score">점수</param>
        public void SetScore(int score)
        {
            if (_scoreText != null)
            {
                _scoreText.SetText(SCORE_FORMAT, score);
            }
        }

        private void SetPhaseStatusVisible(bool isVisible)
        {
            SetObjectVisible(_phaseStatusRoot != null ? _phaseStatusRoot : GetObject(_phaseStatusText), isVisible);
        }

        private void SetRespawnVisible(bool isVisible)
        {
            SetObjectVisible(_respawnRoot != null ? _respawnRoot : GetObject(_respawnText), isVisible);
        }

        private static GameObject GetObject(TMP_Text text)
        {
            return text != null ? text.gameObject : null;
        }

        private static void SetObjectVisible(GameObject target, bool isVisible)
        {
            if (target != null && target.activeSelf != isVisible)
            {
                target.SetActive(isVisible);
            }
        }

        /// <summary>
        /// 슬라이더를 current / max 비율로 맞춘다 (LoadingView와 같이 SetValueWithoutNotify). 최댓값이 0 이하이면 비운다.
        /// 슬라이더의 Min/Max Value가 0 ~ 1이 아니어도 그 범위 안에서 비율대로 놓는다.
        /// </summary>
        private static void SetSliderRatio(Slider slider, int current, int max)
        {
            if (slider != null)
            {
                float ratio = max > 0 ? Mathf.Clamp01((float)current / max) : 0f;
                slider.SetValueWithoutNotify(Mathf.Lerp(slider.minValue, slider.maxValue, ratio));
            }
        }
    }
}
