using UnityEngine;

namespace Project05.View
{
    /// <summary>
    /// 적 프리팹 컴포넌트. 표현용 참조(Renderer)와 피격 플래시 설정을 보관한다. 로직 없음.
    /// 플래시를 언제 켜고 끌지는 EnemyViewSystem이 정하고, 이 컴포넌트는 색을 바꾸는 일만 한다.
    /// </summary>
    public class EnemyView : MonoBehaviour
    {
        [SerializeField] private Renderer[] _renderers;
        [SerializeField] private Color _hitFlashColor = Color.red;
        // 피격 플래시 유지 시간 (틱)
        [SerializeField, Min(1)] private int _hitFlashTicks = 5;

        private MaterialPropertyBlock _propertyBlock;

        // URP Lit·Unlit의 기본 색 속성과 내장 셰이더의 색 속성 (어느 셰이더를 써도 칠해지도록 둘 다 설정)
        private static readonly int BASE_COLOR_PROPERTY_ID = Shader.PropertyToID("_BaseColor");
        private static readonly int COLOR_PROPERTY_ID = Shader.PropertyToID("_Color");

        /// <summary>
        /// 피격 플래시 유지 시간 (틱)
        /// </summary>
        public int HitFlashTicks
        {
            get { return _hitFlashTicks; }
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

        private void Reset()
        {
            _renderers = GetComponentsInChildren<Renderer>(true);
        }

        private void Awake()
        {
            // 프리팹에서 지정하지 않았으면 자식의 Renderer를 모두 쓴다 (생성 시 1회)
            if (_renderers == null || _renderers.Length == 0)
            {
                _renderers = GetComponentsInChildren<Renderer>(true);
            }
        }
    }
}
