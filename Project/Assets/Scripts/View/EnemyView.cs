using UnityEngine;

namespace Project05.View
{
    /// <summary>
    /// 적 프리팹 컴포넌트. 표현용 참조(Renderer, Animator)와 피격 플래시·사망 연출 설정을 보관한다. 로직 없음.
    /// 플래시를 언제 켜고 끌지, 어떤 애니메이션을 재생할지는 EnemyViewSystem이 정하고, 이 컴포넌트는 그대로 반영만 한다.
    /// Animator 파라미터 계약: Speed(float, 0 = 멈춤 ~ 1 = 이동 속도), Attack·Die(trigger).
    /// Controller에 없는 파라미터는 건너뛴다.
    /// </summary>
    public class EnemyView : MonoBehaviour
    {
        [SerializeField] private Renderer[] _renderers;
        [SerializeField] private Color _hitFlashColor = Color.red;
        // 피격 플래시 유지 시간 (틱)
        [SerializeField, Min(1)] private int _hitFlashTicks = 5;
        // 적 애니메이터 (비우면 자식에서 찾는다. 없으면 애니메이션 없이 진행)
        [SerializeField] private Animator _animator;
        // 사망 애니메이션을 보여 준 뒤 풀에 반납할 때까지의 시간 (틱). Animator가 없으면 바로 반납한다
        [SerializeField, Min(0)] private int _corpseTicks = 50;

        private MaterialPropertyBlock _propertyBlock;
        private bool _isAnimationPaused;
        private bool _hasSpeedParameter;
        private bool _hasAttackParameter;
        private bool _hasDieParameter;

        // URP Lit·Unlit의 기본 색 속성과 내장 셰이더의 색 속성 (어느 셰이더를 써도 칠해지도록 둘 다 설정)
        private static readonly int BASE_COLOR_PROPERTY_ID = Shader.PropertyToID("_BaseColor");
        private static readonly int COLOR_PROPERTY_ID = Shader.PropertyToID("_Color");
        private static readonly int SPEED_PARAMETER_ID = Animator.StringToHash("Speed");
        private static readonly int ATTACK_PARAMETER_ID = Animator.StringToHash("Attack");
        private static readonly int DIE_PARAMETER_ID = Animator.StringToHash("Die");

        /// <summary>
        /// 피격 플래시 유지 시간 (틱)
        /// </summary>
        public int HitFlashTicks
        {
            get { return _hitFlashTicks; }
        }

        /// <summary>
        /// 사망 후 풀에 반납하기 전까지 남겨 두는 시간 (틱). Animator가 없으면 0이라 바로 반납한다.
        /// </summary>
        public int CorpseTicks
        {
            get { return _animator != null ? _corpseTicks : 0; }
        }

        /// <summary>
        /// 피격 플래시 색을 켜거나 끈다. 끄면 머티리얼 원래 색으로 돌아간다.
        /// </summary>
        /// <param name="isFlashing">플래시를 켜면 true</param>
        public void SetHitFlash(bool isFlashing)
        {
            if (_propertyBlock == null)
            {
                _propertyBlock = new MaterialPropertyBlock();
            }

            for (int rendererIndex = 0; rendererIndex < _renderers.Length; rendererIndex++)
            {
                Renderer targetRenderer = _renderers[rendererIndex];
                if (targetRenderer == null)
                {
                    continue;
                }

                if (isFlashing)
                {
                    targetRenderer.GetPropertyBlock(_propertyBlock);
                    _propertyBlock.SetColor(BASE_COLOR_PROPERTY_ID, _hitFlashColor);
                    _propertyBlock.SetColor(COLOR_PROPERTY_ID, _hitFlashColor);
                    targetRenderer.SetPropertyBlock(_propertyBlock);
                }
                else
                {
                    targetRenderer.SetPropertyBlock(null);
                }
            }
        }

        /// <summary>
        /// 풀에서 꺼낸 직후 애니메이터를 처음 상태로 되돌린다 (이전 적의 사망 자세가 남지 않게).
        /// </summary>
        public void ResetAnimation()
        {
            if (_animator == null)
            {
                return;
            }

            _isAnimationPaused = false;
            _animator.speed = 1f;
            _animator.Rebind();
            _animator.Update(0f);
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
        /// 코어·타워 공격 애니메이션을 재생한다.
        /// </summary>
        public void PlayAttack()
        {
            if (_animator != null && _hasAttackParameter)
            {
                _animator.SetTrigger(ATTACK_PARAMETER_ID);
            }
        }

        /// <summary>
        /// 사망 애니메이션을 재생한다. 남은 공격 트리거는 지우고 이동 블렌드는 0으로 둔다.
        /// </summary>
        public void PlayDie()
        {
            if (_animator == null)
            {
                return;
            }

            if (_hasAttackParameter)
            {
                _animator.ResetTrigger(ATTACK_PARAMETER_ID);
            }
            SetMoveSpeedRatio(0f);
            if (_hasDieParameter)
            {
                _animator.SetTrigger(DIE_PARAMETER_ID);
            }
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

        private void Reset()
        {
            _renderers = GetComponentsInChildren<Renderer>(true);
            _animator = GetComponentInChildren<Animator>(true);
        }

        private void Awake()
        {
            // 프리팹에서 지정하지 않았으면 자식의 Renderer를 모두 쓴다 (생성 시 1회)
            if (_renderers == null || _renderers.Length == 0)
            {
                _renderers = GetComponentsInChildren<Renderer>(true);
            }
            if (_animator == null)
            {
                _animator = GetComponentInChildren<Animator>(true);
            }
            CacheParameters();
        }

        /// <summary>
        /// Controller에 있는 파라미터만 쓰도록 한 번 확인해 둔다 (없는 파라미터를 쓰면 매번 경고가 난다).
        /// </summary>
        private void CacheParameters()
        {
            if (_animator == null || _animator.runtimeAnimatorController == null)
            {
                _animator = null;
                return;
            }

            AnimatorControllerParameter[] parameters = _animator.parameters;
            for (int parameterIndex = 0; parameterIndex < parameters.Length; parameterIndex++)
            {
                AnimatorControllerParameter parameter = parameters[parameterIndex];
                int nameHash = parameter.nameHash;
                if (nameHash == SPEED_PARAMETER_ID && parameter.type == AnimatorControllerParameterType.Float)
                {
                    _hasSpeedParameter = true;
                }
                else if (nameHash == ATTACK_PARAMETER_ID && parameter.type == AnimatorControllerParameterType.Trigger)
                {
                    _hasAttackParameter = true;
                }
                else if (nameHash == DIE_PARAMETER_ID && parameter.type == AnimatorControllerParameterType.Trigger)
                {
                    _hasDieParameter = true;
                }
            }
        }
    }
}
