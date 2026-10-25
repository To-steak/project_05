using UnityEngine;

namespace Project05.View
{
    /// <summary>
    /// 히어로 주위를 도는 구체 프리팹 컴포넌트. 로직 없음.
    /// 몇 개를 어디에 둘지는 HeroViewSystem이 Logic의 구체 수·각도로 정하고, 풀에서 꺼내 위치만 옮긴다.
    /// 프리팹 원점이 투사체 발사 지점이므로, 보이는 높이를 바꾸려면 자식 오브젝트를 올린다 (투사체 View와 같은 높이로 맞춘다).
    /// </summary>
    public class HeroOrbView : MonoBehaviour
    {
    }
}
