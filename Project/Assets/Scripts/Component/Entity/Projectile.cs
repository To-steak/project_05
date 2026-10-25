using UnityEngine;

namespace Project05.Component.Entity
{
    /// <summary>
    /// 직선 투사체. 발사 방향으로 매 틱 speed만큼 나아가며, 이번 틱 이동 구간에 처음 닿은 적 하나에게 피해를 주고 사라진다.
    /// 최대 비행 거리를 다 쓰면 피해 없이 사라진다. 발사 시 값을 복사하므로 매 틱 테이블을 보지 않는다.
    /// </summary>
    public struct Projectile
    {
        // 이전 틱의 위치 (View 보간용)
        public Vector3 previousPosition;
        // 현재 틱의 위치
        public Vector3 currentPosition;
        // 비행 방향 (XZ 평면 단위 벡터)
        public Vector3 direction;
        // 이동 속도 (유닛 / 틱)
        public float speed;
        // 명중 판정 반경 (적 반경과 더해 판정)
        public float radius;
        // 남은 비행 거리
        public float remainingDistance;
        // 명중 시 피해량
        public int damage;
        // 폭발 반경 (0이면 단일 명중. 0보다 크면 명중·소진 지점에서 반경 안 적 전부에게 피해, 탑승 사격)
        public float splashRadius;
    }
}
