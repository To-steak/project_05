using UnityEngine;

namespace Project05.Foundation
{
    /// <summary>
    /// View용 테이블 (App.ViewData의 타입). Definition의 프리팹·VFX 참조를 게임 시작 시 DefinitionBaker가 채운다.
    /// 로직용 GameData와 나눠 두어 System이 GameObject를 알지 못하게 한다 (DESIGN 5.2 베이킹). 읽기 전용으로 쓴다.
    /// </summary>
    public class ViewData
    {
        // 적 종류별 View 프리팹 (인덱스 = 적 종류, GameData.enemyStats와 같음. 비어 있으면 null)
        public GameObject[] enemyPrefabs = new GameObject[0];
        // 히어로 원거리 투사체 View 프리팹 (비어 있으면 null)
        public GameObject heroProjectilePrefab;
    }
}
