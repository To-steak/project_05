using UnityEngine;

namespace Project05.View
{
    /// <summary>
    /// 히어로 프리팹 컴포넌트. Renderer 등 표현용 참조와 무적 깜빡임 설정을 보관한다. 로직 없음.
    /// 언제 보이고 숨길지는 HeroViewSystem이 정하고, 이 컴포넌트는 Renderer를 켜고 끄는 일만 한다.
    /// </summary>
    public class HeroView : MonoBehaviour
    {
        [SerializeField] private Renderer[] _renderers;
        // 무적 중 깜빡임 한 번(보임 또는 숨김)의 길이 (틱)
        [SerializeField, Min(1)] private int _invincibilityBlinkTicks = 5;

        private bool _isVisible = true;

        /// <summary>
        /// 무적 중 깜빡임 한 번(보임 또는 숨김)의 길이 (틱)
        /// </summary>
        public int InvincibilityBlinkTicks
        {
            get { return _invincibilityBlinkTicks; }
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

        private void Reset()
        {
            _renderers = GetComponentsInChildren<Renderer>(true);
        }

        private void Awake()
        {
            // 프리팹에서 지정하지 않았으면 자식의 Renderer를 모두 쓴다
            if (_renderers == null || _renderers.Length == 0)
            {
                _renderers = GetComponentsInChildren<Renderer>(true);
            }
        }
    }
}
