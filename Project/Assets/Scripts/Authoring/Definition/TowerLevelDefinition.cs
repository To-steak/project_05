using System;
using UnityEngine;

namespace Project05.Authoring.Definition
{
    /// <summary>
    /// 타워 한 단계의 수치. TowerDefinition.levels의 원소로 인스펙터에서 편집한다. DefinitionBaker가 TowerLevelStats로 베이킹한다.
    /// </summary>
    [Serializable]
    public class TowerLevelDefinition
    {
        // 이 단계의 최대 체력
        [Min(1)] public int maxHP = 1;
        // 이 단계의 공격력 (미탑승 자동 공격)
        [Min(0)] public int attackDamage;
        // 이 단계의 사거리 (투사체 최대 비행 거리이기도 하다)
        [Min(0f)] public float range;
        // 이 단계의 공격 주기 (틱)
        [Min(1)] public int attackIntervalTicks = 1;
        // 이 단계에서 다음 단계로 올리는 비용 (마지막 단계는 사용하지 않음)
        [Min(0)] public int upgradeCost;
    }
}
