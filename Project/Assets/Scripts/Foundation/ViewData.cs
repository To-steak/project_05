using System;
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
        // 골드 드롭 View 프리팹 (비어 있으면 null)
        public GameObject goldDropPrefab;
        // XP 드롭 View 프리팹 (비어 있으면 null)
        public GameObject xpDropPrefab;
        // 레벨업 선택지 표시 이름 (인덱스 = GameData.levelUpOptions와 같음)
        public string[] levelUpOptionNames = new string[0];
        // 타워 종류별 View 프리팹 (인덱스 = GameData.towerStats와 같음, 비어 있으면 null)
        public GameObject[] towerPrefabs = new GameObject[0];
        // 타워 종류별 건설 목록 아이콘 (비어 있으면 null)
        public Sprite[] towerIcons = new Sprite[0];
        // 타워 종류별 표시 이름
        public string[] towerNames = new string[0];
        // 적 종류별 사망 효과 프리팹 (인덱스 = 적 종류, 비어 있으면 null)
        public GameObject[] enemyDeathEffectPrefabs = new GameObject[0];
        // 한 번 재생하는 효과 프리팹 (인덱스 = VisualEffectKind, 비어 있으면 null)
        public readonly GameObject[] visualEffectPrefabs = new GameObject[Enum.GetValues(typeof(VisualEffectKind)).Length];
        // 골드 획득 문구 프리팹 (비어 있으면 null)
        public GameObject goldTextPrefab;
        // XP 획득 문구 프리팹 (비어 있으면 null)
        public GameObject XPTextPrefab;
        // 골드 획득 문구 형식 ({0} = 획득량)
        public string goldTextFormat = string.Empty;
        // XP 획득 문구 형식 ({0} = 획득량)
        public string XPTextFormat = string.Empty;
    }
}
