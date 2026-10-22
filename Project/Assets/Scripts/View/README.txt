[View]
네임스페이스: Project05.View

공통 표현 관심사: 애니메이션, VFX, SFX, 카메라, 보간. Update/LateUpdate에서만 동작. World는 읽기만. 어셈블리 참조: Component.

들어갈 파일:
- ViewEvents.cs (static class) : Runner가 이벤트 큐를 비울 때 호출하는 View용 C# event 모음.
- EntityViewRegistry.cs (class) : EntityID → View 오브젝트 매핑.
- InterpolatedView.cs (MonoBehaviour) : previousPosition/currentPosition 보간 공통 베이스.
- HeroView.cs (MonoBehaviour) : 히어로 보간, 이동·공격 애니메이션, 공격 VFX·SFX.
- EnemyView.cs (MonoBehaviour) : 적 보간, 피격 플래시, 이동 애니메이션.
- TowerView.cs (MonoBehaviour) : 타워 회전, 사격 VFX·SFX, 탑승 연출.
- DropView.cs (MonoBehaviour) : 드롭 표시, 획득 연출.
- CoreView.cs (MonoBehaviour) : 코어 피격 연출.
- EffectSpawner.cs (MonoBehaviour) : 사망·히트 VFX를 이벤트 위치에 재생.
- BoardPromptView.cs (MonoBehaviour) : 탑승 가능 범위에서 Space 아이콘 표시.
- OffscreenIndicatorView.cs (MonoBehaviour) : 화면 밖에서 코어로 다가오는 적 방향 화살표.
- HeroCameraView.cs (MonoBehaviour) : Cinemachine 추적 대상 연결, 탑승 시 카메라 전환.

구현하면서 이 목록과 실제 파일을 맞춘다. 컨벤션은 저장소 루트의 CONVENTIONS.md를 따른다.
