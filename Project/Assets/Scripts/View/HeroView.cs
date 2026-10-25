using UnityEngine;

namespace Project05.View
{
    /// <summary>
    /// 히어로 프리팹 컴포넌트. Renderer·Animator 참조, 무적 깜빡임과 사망 연출 설정을 보관한다. 로직 없음.
    /// 언제 보이고 숨길지, 어떤 애니메이션을 재생할지는 HeroViewSystem이 정하고, 이 컴포넌트는 그대로 반영만 한다.
    /// Animator 파라미터 계약: Speed(float, 0 = 멈춤 ~ 1 = 이동 속도), MeleeAttackIndex(int, 0부터 차례로 돈다),
    /// MeleeAttack·Hit·Die·Respawn·LevelUp·Victory(trigger). Controller에 없는 파라미터는 건너뛴다.
    /// 상체 레이어(아바타 마스크)가 있으면 사망 중에는 가중치를 0으로 내려 사망 자세를 덮지 않게 한다.
    /// </summary>
    public class HeroView : MonoBehaviour
    {
        [SerializeField] private Renderer[] _renderers;
        // 무적 중 깜빡임 한 번(보임 또는 숨김)의 길이 (틱)
        [SerializeField, Min(1)] private int _invincibilityBlinkTicks = 5;
        // 히어로 애니메이터 (비우면 자식에서 찾는다. 없으면 애니메이션 없이 진행)
        [SerializeField] private Animator _animator;
        // 상체 레이어 이름 (근거리 공격·피격을 이동과 겹쳐 재생하는 아바타 마스크 레이어. 없으면 무시)
        [SerializeField] private string _upperBodyLayerName = "Upper Body";
        // 사망 후 숨기기 전까지 사망 애니메이션을 보여 주는 시간 (틱). Animator가 없으면 바로 숨긴다
        [SerializeField, Min(0)] private int _deathVisibleTicks = 50;
        // 근거리 공격 동작 수 (MeleeAttackIndex가 0 ~ 이 값 - 1을 차례로 돈다. Attack01~03이면 3)
        [SerializeField, Min(1)] private int _meleeAttackVariationCount = 3;

        private bool _isVisible = true;
        private bool _isAnimationPaused;
        private int _upperBodyLayerIndex = -1;
        private bool _isUpperBodyEnabled = true;
        private bool _hasSpeedParameter;
        private bool _hasMeleeAttackIndexParameter;
        private bool _hasMeleeAttackParameter;
        private bool _hasHitParameter;
        private bool _hasDieParameter;
        private bool _hasRespawnParameter;
        private bool _hasLevelUpParameter;
        private bool _hasVictoryParameter;

        private static readonly int SPEED_PARAMETER_ID = Animator.StringToHash("Speed");
        private static readonly int MELEE_ATTACK_INDEX_PARAMETER_ID = Animator.StringToHash("MeleeAttackIndex");
        private static readonly int MELEE_ATTACK_PARAMETER_ID = Animator.StringToHash("MeleeAttack");
        private static readonly int HIT_PARAMETER_ID = Animator.StringToHash("Hit");
        private static readonly int DIE_PARAMETER_ID = Animator.StringToHash("Die");
        private static readonly int RESPAWN_PARAMETER_ID = Animator.StringToHash("Respawn");
        private static readonly int LEVEL_UP_PARAMETER_ID = Animator.StringToHash("LevelUp");
        private static readonly int VICTORY_PARAMETER_ID = Animator.StringToHash("Victory");

        /// <summary>
        /// 무적 중 깜빡임 한 번(보임 또는 숨김)의 길이 (틱)
        /// </summary>
        public int InvincibilityBlinkTicks
        {
            get { return _invincibilityBlinkTicks; }
        }

        /// <summary>
        /// 사망 애니메이션을 보여 주는 시간 (틱). Animator가 없으면 0이라 사망 즉시 숨긴다.
        /// </summary>
        public int DeathVisibleTicks
        {
            get { return _animator != null ? _deathVisibleTicks : 0; }
        }

        /// <summary>
        /// 근거리 공격 동작 수 (1 이상)
        /// </summary>
        public int MeleeAttackVariationCount
        {
            get { return _meleeAttackVariationCount; }
        }

        /// <summary>
        /// 히어로 Renderer를 모두 보이거나 숨긴다. 상태가 같으면 아무것도 하지 않는다.
        /// </summary>
        /// <param name="isVisible">보이면 true</param>
        public void SetVisible(bool isVisible)
        {
            if (_isVisible == isVisible)
            {
                return;
            }

            _isVisible = isVisible;
            for (int rendererIndex = 0; rendererIndex < _renderers.Length; rendererIndex++)
            {
                if (_renderers[rendererIndex] != null)
                {
                    _renderers[rendererIndex].enabled = isVisible;
                }
            }
        }

        /// <summary>
        /// 이동 블렌드 값을 넣는다.
        /// </summary>
        /// <param name="speedRatio">이번 틱 이동 거리 ÷ 이동 속도 (0 = 멈춤, 1 = 최고 속도)</param>
        public void SetMoveSpeedRatio(float speedRatio)
        {
            if (_animator != null && _hasSpeedParameter)
            {
                _animator.SetFloat(SPEED_PARAMETER_ID, speedRatio);
            }
        }

        /// <summary>
        /// 근거리 공격 애니메이션을 재생한다.
        /// </summary>
        /// <param name="variationIndex">재생할 동작 번호 (0 ~ MeleeAttackVariationCount - 1)</param>
        public void PlayMeleeAttack(int variationIndex)
        {
            if (_animator == null)
            {
                return;
            }
            if (_hasMeleeAttackIndexParameter)
            {
                _animator.SetInteger(MELEE_ATTACK_INDEX_PARAMETER_ID, variationIndex);
            }
            SetTrigger(MELEE_ATTACK_PARAMETER_ID, _hasMeleeAttackParameter);
        }

        /// <summary>
        /// 피격 애니메이션을 재생한다.
        /// </summary>
        public void PlayHit()
        {
            SetTrigger(HIT_PARAMETER_ID, _hasHitParameter);
        }

        /// <summary>
        /// 레벨업 애니메이션을 재생한다.
        /// </summary>
        public void PlayLevelUp()
        {
            SetTrigger(LEVEL_UP_PARAMETER_ID, _hasLevelUpParameter);
        }

        /// <summary>
        /// 승리(스테이지 클리어) 애니메이션을 재생한다.
        /// </summary>
        public void PlayVictory()
        {
            SetTrigger(VICTORY_PARAMETER_ID, _hasVictoryParameter);
        }

        /// <summary>
        /// 사망 애니메이션을 재생한다. 남은 다른 트리거는 지워 사망이 끊기거나 바로 풀리지 않게 한다.
        /// </summary>
        public void PlayDie()
        {
            if (_animator == null)
            {
                return;
            }
            ResetTriggers();
            SetTrigger(DIE_PARAMETER_ID, _hasDieParameter);
        }

        /// <summary>
        /// 부활 애니메이션을 재생한다. 남은 다른 트리거는 지운다.
        /// </summary>
        public void PlayRespawn()
        {
            if (_animator == null)
            {
                return;
            }
            ResetTriggers();
            SetTrigger(RESPAWN_PARAMETER_ID, _hasRespawnParameter);
        }

        /// <summary>
        /// 상체 레이어 가중치를 켜거나 끈다 (사망 중에는 끈다). 레이어가 없거나 상태가 같으면 아무것도 하지 않는다.
        /// </summary>
        /// <param name="isEnabled">켜면 true (가중치 1)</param>
        public void SetUpperBodyEnabled(bool isEnabled)
        {
            if (_animator == null || _upperBodyLayerIndex < 0 || _isUpperBodyEnabled == isEnabled)
            {
                return;
            }

            _isUpperBodyEnabled = isEnabled;
            _animator.SetLayerWeight(_upperBodyLayerIndex, isEnabled ? 1f : 0f);
        }

        /// <summary>
        /// 월드가 멈춘 동안 애니메이션을 그 자리에 멈추고, 다시 움직이면 이어 재생한다. 상태가 같으면 아무것도 하지 않는다.
        /// </summary>
        /// <param name="isPaused">멈추면 true</param>
        public void SetAnimationPaused(bool isPaused)
        {
            if (_animator == null || _isAnimationPaused == isPaused)
            {
                return;
            }

            _isAnimationPaused = isPaused;
            _animator.speed = isPaused ? 0f : 1f;
        }

        private void SetTrigger(int parameterID, bool hasParameter)
        {
            if (_animator != null && hasParameter)
            {
                _animator.SetTrigger(parameterID);
            }
        }

        private void ResetTrigger(int parameterID, bool hasParameter)
        {
            if (hasParameter)
            {
                _animator.ResetTrigger(parameterID);
            }
        }

        private void ResetTriggers()
        {
            ResetTrigger(MELEE_ATTACK_PARAMETER_ID, _hasMeleeAttackParameter);
            ResetTrigger(HIT_PARAMETER_ID, _hasHitParameter);
            ResetTrigger(DIE_PARAMETER_ID, _hasDieParameter);
            ResetTrigger(RESPAWN_PARAMETER_ID, _hasRespawnParameter);
            ResetTrigger(LEVEL_UP_PARAMETER_ID, _hasLevelUpParameter);
            ResetTrigger(VICTORY_PARAMETER_ID, _hasVictoryParameter);
        }

        private void Reset()
        {
            _renderers = GetComponentsInChildren<Renderer>(true);
            _animator = GetComponentInChildren<Animator>(true);
        }

        private void Awake()
        {
            // 프리팹에서 지정하지 않았으면 자식의 Renderer를 모두 쓴다
            if (_renderers == null || _renderers.Length == 0)
            {
                _renderers = GetComponentsInChildren<Renderer>(true);
            }
            if (_animator == null)
            {
                _animator = GetComponentInChildren<Animator>(true);
            }
            if (_animator == null || _animator.runtimeAnimatorController == null)
            {
                _animator = null;
                return;
            }

            CacheParameters();
            if (!string.IsNullOrEmpty(_upperBodyLayerName))
            {
                _upperBodyLayerIndex = _animator.GetLayerIndex(_upperBodyLayerName);
            }
        }

        /// <summary>
        /// Controller에 있는 파라미터만 쓰도록 한 번 확인해 둔다 (없는 파라미터를 쓰면 매번 경고가 난다).
        /// </summary>
        private void CacheParameters()
        {
            AnimatorControllerParameter[] parameters = _animator.parameters;
            for (int parameterIndex = 0; parameterIndex < parameters.Length; parameterIndex++)
            {
                AnimatorControllerParameter parameter = parameters[parameterIndex];
                int nameHash = parameter.nameHash;
                switch (parameter.type)
                {
                    case AnimatorControllerParameterType.Float:
                        _hasSpeedParameter = _hasSpeedParameter || nameHash == SPEED_PARAMETER_ID;
                        break;
                    case AnimatorControllerParameterType.Int:
                        _hasMeleeAttackIndexParameter = _hasMeleeAttackIndexParameter || nameHash == MELEE_ATTACK_INDEX_PARAMETER_ID;
                        break;
                    case AnimatorControllerParameterType.Trigger:
                        _hasMeleeAttackParameter = _hasMeleeAttackParameter || nameHash == MELEE_ATTACK_PARAMETER_ID;
                        _hasHitParameter = _hasHitParameter || nameHash == HIT_PARAMETER_ID;
                        _hasDieParameter = _hasDieParameter || nameHash == DIE_PARAMETER_ID;
                        _hasRespawnParameter = _hasRespawnParameter || nameHash == RESPAWN_PARAMETER_ID;
                        _hasLevelUpParameter = _hasLevelUpParameter || nameHash == LEVEL_UP_PARAMETER_ID;
                        _hasVictoryParameter = _hasVictoryParameter || nameHash == VICTORY_PARAMETER_ID;
                        break;
                }
            }
        }
    }
}
