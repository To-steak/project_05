using UnityEngine;

namespace Project05.View
{
    /// <summary>
    /// 한 번 재생하는 효과 프리팹의 루트 컴포넌트. 유지 시간과 파티클 다시 재생만 한다. 로직 없음.
    /// VisualEffectViewSystem이 풀에서 꺼내 위치를 잡고 Restart한 뒤, 유지 시간이 지나면 풀에 돌려준다.
    /// </summary>
    public class VisualEffectView : MonoBehaviour
    {
        // 풀에 돌아가기까지 유지 시간 (틱, 1틱 = 20ms). 파티클이 끝나는 시간보다 조금 길게 둔다
        [SerializeField, Min(1)] private int _lifetimeTicks = 50;
        // 꺼낼 때마다 처음부터 다시 재생할 파티클 (비우면 자식에서 찾는다)
        [SerializeField] private ParticleSystem[] _particleSystems;
        private bool _hasSearchedParticleSystems;

        /// <summary>
        /// 풀에 돌아가기까지 유지 시간 (틱)
        /// </summary>
        public int LifetimeTicks
        {
            get { return _lifetimeTicks; }
        }

        /// <summary>
        /// 파티클을 처음부터 다시 재생한다. 풀에서 다시 꺼낸 인스턴스가 이전 재생 상태를 이어받지 않게 한다.
        /// </summary>
        public void Restart()
        {
            if (!_hasSearchedParticleSystems)
            {
                _hasSearchedParticleSystems = true;
                if (_particleSystems == null || _particleSystems.Length == 0)
                {
                    _particleSystems = GetComponentsInChildren<ParticleSystem>(true);
                }
            }

            for (int particleIndex = 0; particleIndex < _particleSystems.Length; particleIndex++)
            {
                ParticleSystem particle = _particleSystems[particleIndex];
                if (particle == null)
                {
                    continue;
                }
                particle.Clear(false);
                particle.Play(false);
            }
        }
    }
}
