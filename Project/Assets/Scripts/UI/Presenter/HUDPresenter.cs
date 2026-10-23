using Project05.Component;
using Project05.Component.Entity;
using Project05.UI.View;
using UnityEngine;

namespace Project05.UI.Presenter
{
    /// <summary>
    /// [Game] 인게임 HUD. World를 읽어 웨이브·처치 수·준비 시간·보스, 히어로 HP·목숨·부활, 코어 HP, 골드, 레벨·XP, 점수를 표시한다.
    /// Runner가 LateUpdate에서 매 프레임 호출한다. 입력이 없으므로 World에 명령을 넣지 않는다.
    /// 텍스트 메시 재생성을 줄이려고 마지막으로 보낸 값을 기억해 두고, 바뀐 항목만 View에 보낸다.
    /// 패배 후에도 멈춘 World를 그대로 표시한다 (DESIGN 3.5.2).
    /// </summary>
    public class HUDPresenter
    {
        private readonly HUDView _view;
        // 마지막으로 View에 보낸 값 (바뀌었는지 비교용)
        private int _shownStageNumber = UNSHOWN_VALUE;
        private int _shownWaveNumber = UNSHOWN_VALUE;
        private int _shownKillCount = UNSHOWN_VALUE;
        private int _shownWaveEnemyCount = UNSHOWN_VALUE;
        private int _shownPhaseStatus = UNSHOWN_VALUE;
        private int _shownPreparationSeconds = UNSHOWN_VALUE;
        private int _shownHeroHP = UNSHOWN_VALUE;
        private int _shownHeroMaxHP = UNSHOWN_VALUE;
        private int _shownLives = UNSHOWN_VALUE;
        private int _shownRespawnSeconds = UNSHOWN_VALUE;
        private int _shownCoreHP = UNSHOWN_VALUE;
        private int _shownCoreMaxHP = UNSHOWN_VALUE;
        private int _shownGold = UNSHOWN_VALUE;
        private int _shownLevel = UNSHOWN_VALUE;
        private int _shownXP = UNSHOWN_VALUE;
        private int _shownRequiredXP = UNSHOWN_VALUE;
        private int _shownScore = UNSHOWN_VALUE;

        // 표시 값 캐시의 초기값 (어떤 실제 값과도 다르게)
        private const int UNSHOWN_VALUE = int.MinValue;
        // 단계 안내 종류 (캐시 비교용)
        private const int PHASE_STATUS_HIDDEN = 0;
        private const int PHASE_STATUS_PREPARATION = 1;
        private const int PHASE_STATUS_BOSS = 2;
        // 부활 안내를 숨긴 상태를 나타내는 남은 초
        private const int RESPAWN_HIDDEN_SECONDS = -1;

        /// <summary>
        /// HUD View를 받는다. 첫 Present에서 모든 항목을 채운다.
        /// </summary>
        /// <param name="view">씬의 HUD View</param>
        public HUDPresenter(HUDView view)
        {
            _view = view;
        }

        /// <summary>
        /// World를 읽어 바뀐 항목만 HUD에 반영한다.
        /// </summary>
        /// <param name="world">현재 판의 월드 (읽기 전용)</param>
        public void Present(World world)
        {
            PresentWave(world.flow);
            PresentHero(world.hero);
            PresentCore(world.core);
            PresentProgress(world);
        }

        private void PresentWave(in GameFlowState flow)
        {
            if (flow.stageNumber != _shownStageNumber || flow.waveNumber != _shownWaveNumber)
            {
                _shownStageNumber = flow.stageNumber;
                _shownWaveNumber = flow.waveNumber;
                _view.SetWave(flow.stageNumber, flow.waveNumber);
            }

            if (flow.killCount != _shownKillCount || flow.waveEnemyCount != _shownWaveEnemyCount)
            {
                _shownKillCount = flow.killCount;
                _shownWaveEnemyCount = flow.waveEnemyCount;
                _view.SetKillProgress(flow.killCount, flow.waveEnemyCount);
            }

            int phaseStatus = PHASE_STATUS_HIDDEN;
            if (flow.phase == GamePhase.Preparing)
            {
                phaseStatus = PHASE_STATUS_PREPARATION;
            }
            else if (flow.phase == GamePhase.BossInProgress)
            {
                phaseStatus = PHASE_STATUS_BOSS;
            }

            if (phaseStatus == PHASE_STATUS_PREPARATION)
            {
                int preparationSeconds = ToDisplaySeconds(flow.phaseRemainingTicks);
                if (phaseStatus != _shownPhaseStatus || preparationSeconds != _shownPreparationSeconds)
                {
                    _shownPreparationSeconds = preparationSeconds;
                    _view.ShowPreparation(preparationSeconds);
                }
            }
            else if (phaseStatus != _shownPhaseStatus)
            {
                if (phaseStatus == PHASE_STATUS_BOSS)
                {
                    _view.ShowBoss();
                }
                else
                {
                    _view.HidePhaseStatus();
                }
            }
            _shownPhaseStatus = phaseStatus;
        }

        private void PresentHero(in Hero hero)
        {
            if (hero.currentHP != _shownHeroHP || hero.maxHP != _shownHeroMaxHP)
            {
                _shownHeroHP = hero.currentHP;
                _shownHeroMaxHP = hero.maxHP;
                _view.SetHeroHP(hero.currentHP, hero.maxHP);
            }

            if (hero.remainingLives != _shownLives)
            {
                _shownLives = hero.remainingLives;
                _view.SetLives(hero.remainingLives);
            }

            int respawnSeconds = hero.state == HeroState.Dead ? ToDisplaySeconds(hero.respawnRemainingTicks) : RESPAWN_HIDDEN_SECONDS;
            if (respawnSeconds != _shownRespawnSeconds)
            {
                _shownRespawnSeconds = respawnSeconds;
                if (respawnSeconds == RESPAWN_HIDDEN_SECONDS)
                {
                    _view.HideRespawn();
                }
                else
                {
                    _view.ShowRespawn(respawnSeconds);
                }
            }
        }

        private void PresentCore(in CoreState core)
        {
            if (core.currentHP != _shownCoreHP || core.maxHP != _shownCoreMaxHP)
            {
                _shownCoreHP = core.currentHP;
                _shownCoreMaxHP = core.maxHP;
                _view.SetCoreHP(core.currentHP, core.maxHP);
            }
        }

        private void PresentProgress(World world)
        {
            ProgressState progress = world.progress;
            if (progress.gold != _shownGold)
            {
                _shownGold = progress.gold;
                _view.SetGold(progress.gold);
            }

            int requiredXP = GetRequiredXP(world.progression.requiredXPByLevel, progress.level);
            if (progress.level != _shownLevel || progress.currentXP != _shownXP || requiredXP != _shownRequiredXP)
            {
                _shownLevel = progress.level;
                _shownXP = progress.currentXP;
                _shownRequiredXP = requiredXP;
                _view.SetLevel(progress.level, progress.currentXP, requiredXP);
            }

            if (progress.score != _shownScore)
            {
                _shownScore = progress.score;
                _view.SetScore(progress.score);
            }
        }

        /// <summary>
        /// 남은 틱을 화면에 보일 초로 바꾼다 (올림). 표시 전용이며 로직은 틱만 쓴다.
        /// </summary>
        private static int ToDisplaySeconds(int remainingTicks)
        {
            return remainingTicks > 0 ? Mathf.CeilToInt(remainingTicks * Time.fixedDeltaTime) : 0;
        }

        /// <summary>
        /// 다음 레벨까지 필요한 XP. ProgressSystem.GetRequiredXP와 같은 규칙이다
        /// (UI 어셈블리는 System을 참조하지 않으므로 여기서 같은 방식으로 읽는다). 테이블이 비면 0.
        /// </summary>
        private static int GetRequiredXP(int[] requiredXPByLevel, int level)
        {
            if (requiredXPByLevel == null || requiredXPByLevel.Length == 0)
            {
                return 0;
            }
            return requiredXPByLevel[Mathf.Clamp(level - 1, 0, requiredXPByLevel.Length - 1)];
        }
    }
}
