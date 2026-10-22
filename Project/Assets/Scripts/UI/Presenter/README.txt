[UI/Presenter]
네임스페이스: Project05.UI.Presenter

Runner가 순서대로 호출. World를 읽어 UI View 갱신. 버튼 입력은 world.Input에 Command로 적재 (World 직접 변경 금지). Main 씬과 결과 패널 버튼은 App(Settings, Records, Scene)을 사용. 등록 방식: Runner가 판 시작 시 씬에서 View를 찾아 Presenter를 만들어 보관하고, LateUpdate에서 정해진 순서로 Present(world)를 호출한다.

들어갈 파일:
- MainMenuPresenter.cs (class) : [Main] App.Records 표시(웨이브는 n-x로), 시작 → 로딩 패널, 설정 → 설정 패널.
- SettingsPresenter.cs (class) : [Main·Game] Main은 메인 메뉴가, Game은 일시정지 메뉴가 연다. App.Settings 읽기/쓰기, 닫을 때 디스크 기록.
- LoadingPresenter.cs (class) : [Main] App.Scene 비동기 로드 시작, 진행률, 0.9에서 시작 버튼 활성, 클릭 시 씬 활성화.
- HUDPresenter.cs (class) : [Game]
- LevelUpChoicePresenter.cs (class) : [Game] PauseReason이 LevelUpChoice일 때 표시. 선택지가 바뀐 프레임에만 문구 갱신. 버튼은 ChooseLevelUpOption Command 적재.
- BuildMenuPresenter.cs (class) : [Game] PauseReason이 BuildMenu일 때 표시. 해금된 종류만 보이고 골드가 모자라면 버튼을 막음. BuildTower, CloseBuildMenu Command 적재.
- CoreMenuPresenter.cs (class) : [Game] PauseReason이 CoreMenu일 때 표시(열 때 해금 탭부터). 바뀐 칸만 문구 갱신. UnlockTowerType, UpgradeTower, RepairTower, CloseCoreMenu Command 적재.
- PauseMenuPresenter.cs (class) : [Game] PauseReason이 PauseMenu일 때 표시. 계속은 ClosePauseMenu Command, 설정은 설정 패널 열기(메뉴가 닫히면 설정도 저장·닫기), 메인으로는 App.Scene.
- ResultPresenter.cs (class) : [Game] GamePhase가 Defeat일 때 한 번 표시. 재시작·메인으로는 App.Scene 호출. Runner가 판 시작 시 씬의 ResultView로 만들고 LateUpdate에서 호출.
- CommandRejectedPresenter.cs (class) : [Game] CommandRejectedRecord를 읽어 안내 문구 표시.

구현하면서 이 목록과 실제 파일을 맞춘다. 컨벤션은 저장소 루트의 CONVENTIONS.md를 따른다.
