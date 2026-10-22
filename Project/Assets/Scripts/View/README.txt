[View]
네임스페이스: Project05.View

공통 표현 관심사: 애니메이션, VFX, SFX, 카메라, 보간. Runner가 LateUpdate에서 static ~ViewSystem.Present(world, viewContext)를 정해진 순서로 호출한다. World는 읽기만. 이벤트 구독 없음. 어셈블리 참조: Component, Foundation(App), Unity.Cinemachine.

들어갈 파일:
- ViewContext.cs (class) : View 쪽 상태: EntityID → View 매핑(히어로, 코어, 적, 드롭, 타워), 풀 참조, 카메라 참조(HeroCameraView). View 상태가 static 필드에 남지 않도록 인스턴스로 둔다. Game 씬 진입·퇴장 시 비운다.
- HeroViewSystem.cs (static class) : 히어로 보간, 이동·공격 애니메이션, 무적 깜빡임, 사망·부활 연출.
- EnemyViewSystem.cs (static class) : 적 동기화(새 ID는 풀에서 꺼내기, 사라진 ID는 반납), 보간, 피격 플래시, 사망 VFX.
- TowerViewSystem.cs (static class) : 타워 동기화, 사격·피격·파괴·수리·업그레이드 연출, 탑승 연출.
- DropViewSystem.cs (static class) : 드롭 동기화, 획득 연출.
- CoreViewSystem.cs (static class) : 코어 피격 연출, 화면 경고.
- InteractionPromptViewSystem.cs (static class) : 히어로 상호작용 범위 안의 대상(공터 / 타워 / 코어)에 Space 아이콘 표시.
- OffscreenIndicatorViewSystem.cs (static class) : 화면 밖에서 코어로 다가오는 적 방향 화살표.
- CameraViewSystem.cs (static class) : Cinemachine 추적 대상(히어로 View Transform) 연결, 탑승 시 카메라 전환.
- ViewInterpolation.cs (static class) : ViewSystem 공용 보간 비율 계산 (일시정지·패배 중 1 고정). 상태 없음.
- HeroView.cs (MonoBehaviour) : 히어로 프리팹 컴포넌트 (Animator 등 참조 보관). 로직 없음.
- EnemyView.cs (MonoBehaviour) : 적 프리팹 컴포넌트. 로직 없음.
- TowerView.cs (MonoBehaviour) : 타워 프리팹 컴포넌트. 로직 없음.
- DropView.cs (MonoBehaviour) : 드롭 프리팹 컴포넌트. 로직 없음.
- CoreView.cs (MonoBehaviour) : 코어 오브젝트 컴포넌트. 로직 없음.
- HeroCameraView.cs (MonoBehaviour) : 히어로 추적 CinemachineCamera 표시. 탑승 카메라와 구분용. 로직 없음.
- InteractionPromptView.cs (MonoBehaviour) : Space 아이콘 오브젝트.
- OffscreenIndicatorView.cs (MonoBehaviour) : 화살표 오브젝트.

구현하면서 이 목록과 실제 파일을 맞춘다. 컨벤션은 저장소 루트의 CONVENTIONS.md를 따른다.
