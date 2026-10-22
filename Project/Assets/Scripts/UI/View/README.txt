[UI/View]
네임스페이스: Project05.UI.View

UI 화면 표시만 담당. World와 App을 직접 참조하지 않고 Presenter가 넘겨준 값만 표시한다. 버튼 입력은 onClick으로 Presenter에 알린다.

들어갈 파일:
- MainMenuView.cs (MonoBehaviour) : [Main] 시작, 설정 버튼, 최고 점수·최고 도달 웨이브 텍스트(TMP).
- SettingsView.cs (MonoBehaviour) : [Main] BGM/SFX 볼륨 슬라이더(0 ~ 1), 닫기 버튼. 패널 루트를 켜고 끈다.
- LoadingView.cs (MonoBehaviour) : [Main] 로딩 진행률(슬라이더·TMP 텍스트), 시작 버튼. 패널 루트를 켜고 끈다.
- HUDView.cs (MonoBehaviour) : [Game] 골드, XP, 레벨, 스테이지-웨이브, 처치 수 current/max, 코어 HP, 히어로 HP, 목숨. (항목별 분리는 구현 시 결정)
- LevelUpChoiceView.cs (MonoBehaviour) : [Game] 레벨업 3택 1. 버튼 3개와 설명 텍스트(TMP). 항상 켜진 오브젝트에 두고 패널 루트를 켜고 끈다.
- CoreMenuView.cs (MonoBehaviour) : [Game] 코어 UI: 타워 해금, 타워별 업그레이드, 수리, 닫기.
- ResultView.cs (MonoBehaviour) : [Game] 점수·도달 웨이브 텍스트(TMP), 재시작, 메인으로. 항상 켜진 오브젝트에 두고 패널 루트를 켜고 끈다.
- CommandRejectedView.cs (MonoBehaviour) : [Game] 명령 거절 안내 문구 (골드 부족 등).

구현하면서 이 목록과 실제 파일을 맞춘다. 컨벤션은 저장소 루트의 CONVENTIONS.md를 따른다.
