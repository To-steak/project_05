using System.Collections.Generic;
using Project05.Component;
using UnityEngine;

namespace Project05.View
{
    /// <summary>
    /// 한 판의 화면 상태. EntityID → View 오브젝트 매핑, 씬의 표현용 참조(코어 View 등)를 담는다.
    /// Runner가 Game 씬 진입 시 만들고 퇴장 시 버린다. ViewSystem은 static 필드 대신 이 인스턴스만 쓴다.
    /// 풀·카메라 참조는 해당 ViewSystem을 구현하는 브랜치에서 추가한다 (PR 리뷰 포인트에 명시).
    /// </summary>
    public class ViewContext
    {
        // 히어로 View (씬에 없으면 null)
        public HeroView heroView;
        // 히어로 둘레에 놓인 구체 View (HeroViewSystem이 Logic의 구체 수에 맞춰 풀에서 꺼내고 반납한다)
        public readonly List<HeroOrbView> heroOrbViews = new List<HeroOrbView>(MAX_HERO_ORB_VIEW_CAPACITY);
        // 히어로 추적 Cinemachine 카메라 (씬에 없으면 null)
        public HeroCameraView heroCameraView;
        // 코어 View (씬에 없으면 null)
        public CoreView coreView;
        // Q/E 카메라 회전 입력 (-1 = Q 왼쪽, 1 = E 오른쪽, 0 = 없음). Runner가 Update에서 넣는다
        public float cameraRotateInput;
        // 판 시작 시의 Cinemachine Follow 오프셋 (회전 기준, 씬에서 정한 값)
        public Vector3 cameraBaseFollowOffset;
        // 판 시작 시 기준 오프셋을 읽었는지
        public bool hasCameraBaseFollowOffset;
        // 지금까지 돌린 카메라 Y축 각도 (도)
        public float cameraYawDegrees;
        // 화면을 그리는 메인 카메라 (화면 밖 판정, 아이콘 빌보드. 없으면 null)
        public Camera mainCamera;
        // Space 아이콘 (씬에 없으면 null)
        public InteractionPromptView interactionPromptView;
        // 화면 밖 적 화살표 묶음 (씬에 없으면 null)
        public OffscreenIndicatorView offscreenIndicatorView;
        // 판 시작 시 만든 화면 밖 적 화살표 (없으면 빈 배열)
        public RectTransform[] offscreenArrows = new RectTransform[0];
        // 화살표마다 고른 적의 화면 좌표 (화살표 수만큼, 매 프레임 재사용)
        public Vector3[] offscreenScreenPoints = new Vector3[0];
        // 화살표마다 고른 적의 코어까지 거리 제곱 (가까운 순 선택용, 매 프레임 재사용)
        public float[] offscreenSqrDistances = new float[0];
        // 다음 근거리 공격 동작 번호 (HeroViewSystem이 MeleeAttackIndex로 차례로 돌린다)
        public int heroMeleeAttackCount;
        // 레벨업 효과음 기준 레벨 (이보다 오르면 효과음을 낸다, AudioViewSystem)
        public int audioHeardLevel;
        // audioHeardLevel을 판 시작 후 읽었는지 (첫 프레임에는 효과음 없이 기준만 잡는다)
        public bool hasAudioHeardLevel;
        // 이번 판의 패배 효과음을 냈는지 (Defeat는 기록이 아니라 상태라 한 번만 내도록)
        public bool hasPlayedDefeatSound;
        // 살아있는 적 ID → 적 View
        public readonly Dictionary<EntityID, EnemyView> enemyViews = new Dictionary<EntityID, EnemyView>(World.MAX_ENEMY_COUNT);
        // 살아있는 드롭 ID → 드롭 View
        public readonly Dictionary<EntityID, DropView> dropViews = new Dictionary<EntityID, DropView>(World.MAX_DROP_COUNT);
        // 살아있는 투사체 ID → 투사체 View
        public readonly Dictionary<EntityID, ProjectileView> projectileViews = new Dictionary<EntityID, ProjectileView>(World.MAX_PROJECTILE_COUNT);
        // 피격 플래시 중인 적 ID → 플래시가 끝나는 Time.time
        public readonly Dictionary<EntityID, float> enemyHitFlashEndTimes = new Dictionary<EntityID, float>(World.MAX_ENEMY_COUNT);
        // 사망 애니메이션 중인 적 View (EnemyViewSystem이 반납 시각이 지나면 풀에 돌려준다)
        public readonly List<ActiveEnemyCorpse> activeEnemyCorpses = new List<ActiveEnemyCorpse>(World.MAX_ENEMY_COUNT);
        // 살아있는 타워 ID → 타워 View
        public readonly Dictionary<EntityID, TowerView> towerViews = new Dictionary<EntityID, TowerView>();
        // 코어 피격 화면 경고 (씬에 없으면 null)
        public CoreWarningView coreWarningView;
        // 코어 피격 화면 경고가 끝나는 Time.time (CoreViewSystem)
        public float coreWarningEndTime;
        // 재생 중인 한 번짜리 효과 (VisualEffectViewSystem이 유지 시간이 지나면 풀에 돌려준다)
        public readonly List<ActiveVisualEffect> activeVisualEffects = new List<ActiveVisualEffect>(World.MAX_ENEMY_COUNT);
        // 재생 중인 효과를 월드 정지에 맞춰 멈춰 두었는지 (VisualEffectViewSystem)
        public bool areVisualEffectsPaused;
        // 떠오르는 중인 문구
        public readonly List<ActiveFloatingText> activeFloatingTexts = new List<ActiveFloatingText>(World.MAX_DROP_COUNT);
        // 획득량 → 골드 획득 문구 (같은 양의 문구를 매번 새로 만들지 않도록)
        public readonly Dictionary<int, string> goldTextByAmount = new Dictionary<int, string>();
        // 획득량 → XP 획득 문구
        public readonly Dictionary<int, string> XPTextByAmount = new Dictionary<int, string>();
        // 동기화 중 사라진 ID를 모으는 재사용 목록 (Dictionary 순회 중 삭제를 피하고 매 프레임 할당을 막는다)
        public readonly List<EntityID> removedEntityIDs = new List<EntityID>(World.MAX_ENEMY_COUNT);

        // 구체 View 목록의 처음 용량 (최대 구체 수는 데이터 값이라 넘으면 목록이 늘어난다)
        private const int MAX_HERO_ORB_VIEW_CAPACITY = 8;
    }
}
