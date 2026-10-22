using System.Collections.Generic;
using Project05.Component.Data;
using Project05.Component.Entity;
using Project05.Component.Input;
using Project05.Component.Path;
using Project05.Component.Record;
using Project05.Component.Request;
using UnityEngine;

namespace Project05.Component
{
    /// <summary>
    /// 한 판의 모든 게임 상태. 판마다 new World()로 만들고 SessionSystem.Initialize로 초기화한다.
    /// 컬렉션(저장소, 큐, 기록 목록)은 여기서 고정 용량으로 미리 만들어 매 틱 할당을 피한다.
    /// 이 파일은 feat/component-contract에서 고정한다. 이후 브랜치에서 필드를 바꾸면 PR 리뷰 포인트에 명시한다.
    /// </summary>
    public class World
    {
        // ───── 시뮬레이션 ─────

        // 판 시작 후 실행된 시뮬레이션 틱 수 (일시정지·패배 중에는 오르지 않음)
        public int tickCount;
        // 게임 흐름 (단계, 스테이지, 웨이브, 처치 진행도, 보스)
        public GameFlowState flow;
        // 일시정지 사유. None이 아니면 CommandSystem만 실행
        public PauseReason pauseReason;
        // 시드 고정 랜덤. System은 UnityEngine.Random 대신 이것만 쓴다 (시드 결정과 생성은 feat/drop-progress)
        public global::System.Random random;

        // ───── 베이킹 테이블 참조 (판 시작 시 App.Data에서 받음, 읽기 전용) ─────

        // 히어로 수치
        public HeroStats heroStats;
        // 적 종류별 수치 (인덱스 = 적 종류)
        public EnemyStats[] enemyStats;
        // 타워 종류별 수치 (인덱스 = 타워 종류)
        public TowerStats[] towerStats;
        // 웨이브 규칙
        public WaveRuleStats waveRule;
        // 난이도 증가 계수
        public DifficultyStats difficulty;
        // 레벨업 선택지 목록
        public LevelUpOptionStats[] levelUpOptions;
        // 레벨 진행
        public ProgressionStats progression;

        // ───── 씬 베이킹 데이터 (판 시작 시 Authoring에서 받음) ─────

        // 적 웨이포인트 경로 (갈래별)
        public WaypointPath[] waypointPaths;
        // 적 스폰 지점
        public SpawnPoint[] spawnPoints;
        // 타워 공터 (인덱스 = 공터 번호)
        public TowerSlot[] towerSlots;
        // 히어로 시작·부활 위치
        public Vector3 heroSpawnPosition;

        // ───── 단일 엔티티와 상태 ─────

        // 히어로
        public Hero hero;
        // 코어
        public CoreState core;
        // 골드, XP, 레벨, 레벨업 선택지, 획득 퍼센트, 점수
        public ProgressState progress;
        // 타워 종류별 해금 상태
        public TowerUnlockState towerUnlock;

        // ───── 엔티티 저장소 ─────

        // 적 저장소 (보스 포함)
        public readonly EntityStore<Enemy> enemies = new EntityStore<Enemy>(MAX_ENEMY_COUNT);
        // 드롭 저장소
        public readonly EntityStore<Drop> drops = new EntityStore<Drop>(MAX_DROP_COUNT);
        // 타워 저장소. 용량 = 공터 수이므로 공터를 받은 뒤 SessionSystem이 만든다
        public EntityStore<Tower> towers;

        // ───── 입력 ─────

        // 입력 버퍼. Runner가 Update에서 쌓고 CommandSystem이 소비
        public readonly InputBuffer input = new InputBuffer();

        // ───── 생성 요청 큐 (SpawnSystem이 틱 마지막에 처리) ─────

        // 적·보스 생성 요청
        public readonly Queue<EnemySpawnRequest> enemySpawnRequests = new Queue<EnemySpawnRequest>(MAX_ENEMY_COUNT);
        // 드롭 생성 요청
        public readonly Queue<DropSpawnRequest> dropSpawnRequests = new Queue<DropSpawnRequest>(MAX_DROP_COUNT);
        // 타워 건설 요청
        public readonly Queue<TowerBuildRequest> towerBuildRequests = new Queue<TowerBuildRequest>(INITIAL_RECORD_CAPACITY);

        // ───── 프레임 기록 목록 (ViewSystem이 읽고 Runner가 LateUpdate 마지막에 비움) ─────

        // 웨이브 시작
        public readonly List<WaveStartedRecord> waveStartedRecords = new List<WaveStartedRecord>(INITIAL_RECORD_CAPACITY);
        // 웨이브 클리어
        public readonly List<WaveClearedRecord> waveClearedRecords = new List<WaveClearedRecord>(INITIAL_RECORD_CAPACITY);
        // 보스 등장
        public readonly List<BossSpawnedRecord> bossSpawnedRecords = new List<BossSpawnedRecord>(INITIAL_RECORD_CAPACITY);
        // 스테이지 클리어
        public readonly List<StageClearedRecord> stageClearedRecords = new List<StageClearedRecord>(INITIAL_RECORD_CAPACITY);
        // 히어로 근거리 광역 공격
        public readonly List<HeroMeleeAttackRecord> heroMeleeAttackRecords = new List<HeroMeleeAttackRecord>(INITIAL_RECORD_CAPACITY);
        // 히어로 원거리 공격
        public readonly List<HeroRangedShotRecord> heroRangedShotRecords = new List<HeroRangedShotRecord>(INITIAL_RECORD_CAPACITY);
        // 히어로 피격
        public readonly List<HeroDamagedRecord> heroDamagedRecords = new List<HeroDamagedRecord>(INITIAL_RECORD_CAPACITY);
        // 히어로 사망
        public readonly List<HeroDiedRecord> heroDiedRecords = new List<HeroDiedRecord>(INITIAL_RECORD_CAPACITY);
        // 히어로 부활
        public readonly List<HeroRespawnedRecord> heroRespawnedRecords = new List<HeroRespawnedRecord>(INITIAL_RECORD_CAPACITY);
        // 타워 건설
        public readonly List<TowerBuiltRecord> towerBuiltRecords = new List<TowerBuiltRecord>(INITIAL_RECORD_CAPACITY);
        // 타워 업그레이드
        public readonly List<TowerUpgradedRecord> towerUpgradedRecords = new List<TowerUpgradedRecord>(INITIAL_RECORD_CAPACITY);
        // 타워 사격
        public readonly List<TowerShotRecord> towerShotRecords = new List<TowerShotRecord>(INITIAL_RECORD_CAPACITY);
        // 타워 피격
        public readonly List<TowerDamagedRecord> towerDamagedRecords = new List<TowerDamagedRecord>(INITIAL_RECORD_CAPACITY);
        // 타워 파괴
        public readonly List<TowerDestroyedRecord> towerDestroyedRecords = new List<TowerDestroyedRecord>(INITIAL_RECORD_CAPACITY);
        // 타워 수리
        public readonly List<TowerRepairedRecord> towerRepairedRecords = new List<TowerRepairedRecord>(INITIAL_RECORD_CAPACITY);
        // 적 피격
        public readonly List<EnemyDamagedRecord> enemyDamagedRecords = new List<EnemyDamagedRecord>(MAX_ENEMY_COUNT);
        // 적 사망
        public readonly List<EnemyDiedRecord> enemyDiedRecords = new List<EnemyDiedRecord>(MAX_ENEMY_COUNT);
        // 적의 코어 공격
        public readonly List<EnemyCoreAttackRecord> enemyCoreAttackRecords = new List<EnemyCoreAttackRecord>(MAX_ENEMY_COUNT);
        // 적의 타워 공격
        public readonly List<EnemyTowerAttackRecord> enemyTowerAttackRecords = new List<EnemyTowerAttackRecord>(MAX_ENEMY_COUNT);
        // 드롭 획득
        public readonly List<DropCollectedRecord> dropCollectedRecords = new List<DropCollectedRecord>(INITIAL_RECORD_CAPACITY);
        // 코어 피격
        public readonly List<CoreDamagedRecord> coreDamagedRecords = new List<CoreDamagedRecord>(INITIAL_RECORD_CAPACITY);
        // 명령 거절
        public readonly List<CommandRejectedRecord> commandRejectedRecords = new List<CommandRejectedRecord>(INITIAL_RECORD_CAPACITY);

        // 적 저장소 용량 (웨이브 최대 100 + 보스 + 코어 포위로 남은 적, DESIGN 5.6)
        public const int MAX_ENEMY_COUNT = 256;
        // 드롭 저장소 용량 (바닥에 동시에 깔릴 양, DESIGN 5.6)
        public const int MAX_DROP_COUNT = 256;
        // 기록 목록·작은 큐의 초기 용량 (넘치면 List가 늘어나므로 상한이 아니다)
        public const int INITIAL_RECORD_CAPACITY = 16;
    }
}
