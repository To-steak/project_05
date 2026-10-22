[UI/View]
네임스페이스: Project05.UI.View

UI 화면 표시만 담당. World와 App을 직접 참조하지 않고 Presenter가 넘겨준 값만 표시한다. 버튼 입력은 Presenter에 알린다.

들어갈 파일:
- MainMenuView.cs (MonoBehaviour) : [Main] 시작, 설정 버튼.
- SettingsView.cs (MonoBehaviour) : [Main] BGM/SFX 볼륨 슬라이더.
- LoadingView.cs (MonoBehaviour) : [Main] 로딩 진행률, 시작 버튼.
- HUDView.cs (MonoBehaviour) : [Game] 골드, XP, 레벨, 웨이브, 코어 HP. (항목별로 나눌지는 구현 시 결정)
- BuildMenuView.cs (MonoBehaviour) : [Game] 빈 슬롯 클릭 시 건설 메뉴.
- UpgradeMenuView.cs (MonoBehaviour) : [Game] 타워 클릭 시 강화 메뉴.
- LevelUpChoiceView.cs (MonoBehaviour) : [Game] 레벨업 3택 1.
- ResultView.cs (MonoBehaviour) : [Game] 승리/패배, 재시작, 메인으로.

구현하면서 이 목록과 실제 파일을 맞춘다. 컨벤션은 저장소 루트의 CONVENTIONS.md를 따른다.
