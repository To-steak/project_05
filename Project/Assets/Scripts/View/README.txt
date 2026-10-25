[View]
네임스페이스: Project05.View

공통 표현 관심사: 애니메이션, VFX, SFX, 카메라, 보간. Runner가 LateUpdate에서 static ~ViewSystem.Present(world, viewContext)를 정해진 순서로 호출한다. World는 읽기만. 이벤트 구독 없음. 어셈블리 참조: Component, Foundation(App), Unity.Cinemachine, Unity.TextMeshPro(떠오르는 문구).

들어갈 파일:
- ViewContext.cs (class) : View 쪽 상태: EntityID → View 매핑(히어로, 코어, 적, 투사체, 드롭, 타워), 적 피격 플래시 종료 시각, 풀 참조, 카메라 참조(HeroCameraView), 오디오용 레벨업·패배 감지 값, 재생 중 효과·문구 목록, 획득 문구 캐시, 코어 경고 View와 종료 시각. View 상태가 static 필드에 남지 않도록 인스턴스로 둔다. Game 씬 진입·퇴장 시 비운다.
- HeroViewSystem.cs (static class) : 히어로 보간, 이동·공격 애니메이션, 무적 깜빡임, 사망·부활 연출. 피격·근거리 범위(반지름 배율)·사망·부활 효과.
- EnemyViewSystem.cs (static class) : 적 동기화(새 ID는 풀에서 꺼내기, 사라진 ID는 반납), 보간, 피격 플래시, 피격 효과, 종류별 사망 효과.
- ProjectileViewSystem.cs (static class) : 투사체 동기화(풀), 보간, 비행 방향.
- TowerViewSystem.cs (static class) : 타워 동기화, 건설·강화·피격·파괴·수리 효과, (예정) 사격·탑승 연출.
- DropViewSystem.cs (static class) : 드롭 동기화, 획득 문구("+10G", 획득량별 문자열 캐시).
- CoreViewSystem.cs (static class) : 코어 피격 효과, 화면 경고(CoreWarningView 세기를 유지 시간 동안 줄임).
- VisualEffectViewSystem.cs (static class) : 한 번짜리 효과·떠오르는 문구 공용. Spawn~으로 App.Pool에서 꺼내고, Present가 효과는 파티클이 끝나면(IsAlive), 문구는 유지 시간이 지나면 반납·문구를 떠올림. 꺼내는 ViewSystem들 다음에 호출.
- InteractionPromptViewSystem.cs (static class) : 지금 Space를 누르면 반응할 대상(World.interactionTargetKind) 위에 Space 아이콘 하나.
- OffscreenIndicatorViewSystem.cs (static class) : 화면 밖 적 중 코어에 가까운 순 최대 N개를 화면 가장자리 방향 화살표로.
- AudioViewSystem.cs (static class) : 배경음 선택(패배 정지, 보스 생존 시 보스, 아니면 일반)과 프레임 기록 → 효과음. 레벨업·패배는 ViewContext 값과 비교해 한 번. App.Audio로 재생.
- CameraViewSystem.cs (static class) : Cinemachine 추적 대상(히어로 View Transform) 연결, Q/E로 Follow 오프셋을 히어로 기준 Y축 회전, 탑승 시 카메라 전환.
- ViewInterpolation.cs (static class) : ViewSystem 공용 보간 비율 계산 (일시정지·패배 중 1 고정), 시뮬레이션이 멈췄는지(효과·문구·경고·피격 플래시도 멈춤). 상태 없음.
- HeroView.cs (MonoBehaviour) : 히어로 프리팹 컴포넌트 (Renderer 참조, 무적 깜빡임 틱). 보이기·숨기기만 한다. 로직 없음.
- EnemyView.cs (MonoBehaviour) : 적 프리팹 컴포넌트. Renderer 참조와 피격 플래시 색·시간 설정, 색 바꾸기만 한다. 로직 없음.
- ProjectileView.cs (MonoBehaviour) : 투사체 프리팹 컴포넌트. 로직 없음.
- TowerView.cs (MonoBehaviour) : 타워 프리팹 컴포넌트. 로직 없음.
- DropView.cs (MonoBehaviour) : 드롭 프리팹 컴포넌트. 로직 없음.
- CoreView.cs (MonoBehaviour) : 코어 오브젝트 컴포넌트. 로직 없음.
- HeroCameraView.cs (MonoBehaviour) : 히어로 추적 CinemachineCamera 표시와 CinemachineFollow 참조, 초당 회전 각도. 판 시작 시 Binding Mode를 World Space로. 로직 없음.
- InteractionPromptView.cs (MonoBehaviour) : Space 아이콘 오브젝트 (높이 오프셋, 카메라 바라보기).
- VisualEffectView.cs (MonoBehaviour) : 효과 프리팹 루트(루트에 ParticleSystem 필수, Looping 끔). 꺼낼 때 루트 파티클 다시 재생, IsAlive로 끝났는지 알려 줌. 로직 없음.
- FloatingTextView.cs (MonoBehaviour) : 떠오르는 문구 프리팹 루트. TMP_Text, 유지 시간(틱), 떠오르는 높이, 흐려지기 시작 진행도. 로직 없음.
- CoreWarningView.cs (MonoBehaviour) : 코어 피격 화면 경고 (Overlay Canvas 아래, CanvasGroup 투명도). 유지 시간(틱), 최대 투명도. 로직 없음.
- ActiveVisualEffect.cs (struct) : 재생 중인 효과 (ViewContext 목록 원소).
- ActiveFloatingText.cs (struct) : 떠오르는 중인 문구, 시작 위치·시각, 반납 시각 (ViewContext 목록 원소).
- OffscreenIndicatorView.cs (MonoBehaviour) : 화면 밖 화살표 묶음 (Overlay Canvas 아래, 화살표 템플릿·최대 수·가장자리 여백). 판 시작 시 화살표를 복제한다.

구현하면서 이 목록과 실제 파일을 맞춘다. 컨벤션은 저장소 루트의 CONVENTIONS.md를 따른다.
