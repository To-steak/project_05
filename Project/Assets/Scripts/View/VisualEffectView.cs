using UnityEngine;

namespace Project05.View
{
    /// <summary>
    /// 한 번 재생하는 효과 프리팹의 루트 컴포넌트. 루트의 ParticleSystem을 다시 재생하고 끝났는지 알려 준다. 로직 없음.
    /// 효과의 길이는 파티클 설정(Duration, Start Lifetime)이 정한다. VisualEffectViewSystem이 끝난 것을 보고 풀에 돌려준다.
    /// 루트에 ParticleSystem이 있어야 하며, Looping은 끈다 (반복 파티클은 끝나지 않아 풀에 돌아가지 않는다).
    /// </summary>
    public class VisualEffectView : MonoBehaviour
    {
        // 루트 파티클 (자식·서브 에미터는 이 파티클이 함께 다룬다)
        private ParticleSystem _rootParticle;
        private bool _hasSearchedRootParticle;

        /// <summary>
        /// 루트에 ParticleSystem이 있는지. 없으면 재생도 끝 판정도 할 수 없다.
        /// </summary>
        public bool HasParticle
        {
            get { return FindRootParticle() != null; }
        }

        /// <summary>
        /// 아직 재생 중인지 (자식 파티클과 남은 입자 포함). 파티클이 없으면 false.
        /// </summary>
        public bool IsAlive
        {
            get
            {
                ParticleSystem rootParticle = FindRootParticle();
                return rootParticle != null && rootParticle.IsAlive(true);
            }
        }

        /// <summary>
        /// 루트 파티클을 자식까지 비우고 처음부터 다시 재생한다. 풀에서 다시 꺼낸 인스턴스가 이전 입자를 이어받지 않게 한다.
        /// </summary>
        public void Restart()
        {
            ParticleSystem rootParticle = FindRootParticle();
            if (rootParticle == null)
            {
                return;
            }

            rootParticle.Clear(true);
            rootParticle.Play(true);
        }

        private ParticleSystem FindRootParticle()
        {
            if (!_hasSearchedRootParticle)
            {
                _hasSearchedRootParticle = true;
                _rootParticle = GetComponent<ParticleSystem>();
            }
            return _rootParticle;
        }
    }
}
