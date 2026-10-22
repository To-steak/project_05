[UI/Presenter]
네임스페이스: Project05.UI.Presenter

Runner가 순서대로 호출. World를 읽어 UI View 갱신. 버튼 입력은 world.Input에 Command로 적재 (World 직접 변경 금지). Main 씬과 결과 패널 버튼은 App(Settings, Records, Scene)을 사용. 등록 방식은 구현 시 결정.

들어갈 파일:
- MainMenuPresenter.cs (class) : [Main] App.Records 표시.
- SettingsPresenter.cs (class) : [Main] App.Settings 읽기/저장.
- LoadingPresenter.cs (class) : [Main] App.Scene 진행률, 시작 시 씬 활성화.
- HUDPresenter.cs (class) : [Game]
- LevelUpChoicePresenter.cs (class) : [Game] PauseReason이 LevelUpChoice일 때 표시. ChooseLevelUpOption Command 적재.
- CoreMenuPresenter.cs (class) : [Game] PauseReason이 CoreMenu일 때 표시. UnlockTowerType, UpgradeTower, RepairTower, CloseCoreMenu Command 적재.
- ResultPresenter.cs (class) : [Game] GamePhase가 Defeat일 때 표시. 재시작·메인으로는 App.Scene 호출.
- CommandRejectedPresenter.cs (class) : [Game] CommandRejectedRecord를 읽어 안내 문구 표시.

구현하면서 이 목록과 실제 파일을 맞춘다. 컨벤션은 저장소 루트의 CONVENTIONS.md를 따른다.
