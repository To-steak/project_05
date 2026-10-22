using UnityEngine;

namespace Project05.Component.Entity
{
    /// <summary>
    /// 건설된 타워. 건설·업그레이드 시 TowerStats 값을 복사한다.
    /// </summary>
    public struct Tower
    {
        // 위치 (공터 위치와 같음)
        public Vector3 position;
        // 건설된 공터 인덱스 (World.towerSlots)
        public int slotIndex;
        // 타워 종류 (TowerStats 테이블 인덱스)
        public int towerTypeIndex;
        // 업그레이드 단계 (0부터)
        public int level;
        // 현재 체력
        public int currentHP;
        // 최대 체력
        public int maxHP;
        // 미탑승 자동 공격력
        public int attackDamage;
        // 사거리
        public float range;
        // 공격 주기 (틱)
        public int attackIntervalTicks;
        // 투사체 이동 속도 (유닛 / 틱, 건설 시 TowerStats에서 복사)
        public float projectileSpeed;
        // 투사체 명중 판정 반경
        public float projectileRadius;
        // 다음 공격까지 남은 틱 (0이면 준비됨)
        public int attackCooldownTicks;
        // 적이 경로를 멈추고 이 타워를 공격하기 시작하는 거리
        public float aggroRange;
        // 포위 자리 사용 현황: 지금까지 배정한 자리 수 (다음 배정 자리 번호)
        public int siegeSlotAssignedCount;
        // 히어로 탑승 여부
        public bool isBoarded;
    }
}
