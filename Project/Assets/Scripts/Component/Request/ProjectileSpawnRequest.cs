using UnityEngine;

namespace Project05.Component.Request
{
    /// <summary>
    /// 투사체 생성 요청. 발사한 System이 값을 모두 채우고, SpawnSystem이 그대로 투사체에 복사한다.
    /// </summary>
    public struct ProjectileSpawnRequest
    {
        // 발사 위치
        public Vector3 startPosition;
        // 비행 방향 (XZ 평면 단위 벡터)
        public Vector3 direction;
        // 이동 속도 (유닛 / 틱)
        public float speed;
        // 명중 판정 반경
        public float radius;
        // 최대 비행 거리
        public float maxDistance;
        // 명중 시 피해량
        public int damage;
        // 폭발 반경 (0이면 첫 적 1명만 맞히는 단일 명중, 0보다 크면 명중·소진 지점에서 폭발)
        public float splashRadius;
    }
}
