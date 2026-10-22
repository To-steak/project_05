using System.Collections.Generic;
using Project05.Component;

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
        // 히어로 추적 Cinemachine 카메라 (씬에 없으면 null)
        public HeroCameraView heroCameraView;
        // 코어 View (씬에 없으면 null)
        public CoreView coreView;
        // 살아있는 적 ID → 적 View
        public readonly Dictionary<EntityID, EnemyView> enemyViews = new Dictionary<EntityID, EnemyView>(World.MAX_ENEMY_COUNT);
        // 살아있는 드롭 ID → 드롭 View
        public readonly Dictionary<EntityID, DropView> dropViews = new Dictionary<EntityID, DropView>(World.MAX_DROP_COUNT);
        // 살아있는 투사체 ID → 투사체 View
        public readonly Dictionary<EntityID, ProjectileView> projectileViews = new Dictionary<EntityID, ProjectileView>(World.MAX_PROJECTILE_COUNT);
        // 피격 플래시 중인 적 ID → 플래시가 끝나는 Time.time
        public readonly Dictionary<EntityID, float> enemyHitFlashEndTimes = new Dictionary<EntityID, float>(World.MAX_ENEMY_COUNT);
        // 살아있는 타워 ID → 타워 View
        public readonly Dictionary<EntityID, TowerView> towerViews = new Dictionary<EntityID, TowerView>();
        // 동기화 중 사라진 ID를 모으는 재사용 목록 (Dictionary 순회 중 삭제를 피하고 매 프레임 할당을 막는다)
        public readonly List<EntityID> removedEntityIDs = new List<EntityID>(World.MAX_ENEMY_COUNT);
    }
}
