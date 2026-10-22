[UI/Presenter]
네임스페이스: Project05.UI.Presenter

UI View와 데이터를 연결. World를 읽고 ViewEvents를 받아 View 갱신. View의 입력은 world.Input에 Command로 적재. Main 씬 Presenter는 App(Settings, Scene)을 사용.

들어갈 파일:
- MainMenuPresenter.cs (class) : [Main]
- SettingsPresenter.cs (class) : [Main] App.Settings 읽기/저장.
- LoadingPresenter.cs (class) : [Main] App.Scene 진행률, 시작 시 씬 활성화.
- HUDPresenter.cs (class) : [Game]
- BuildMenuPresenter.cs (class) : [Game] BuildTower Command 적재.
- UpgradeMenuPresenter.cs (class) : [Game] UpgradeTower Command 적재.
- LevelUpChoicePresenter.cs (class) : [Game] ChooseLevelUpOption Command 적재.
- ResultPresenter.cs (class) : [Game] 재시작, 메인으로.

구현하면서 이 목록과 실제 파일을 맞춘다. 컨벤션은 저장소 루트의 CONVENTIONS.md를 따른다.
