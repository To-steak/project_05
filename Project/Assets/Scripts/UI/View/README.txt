[UI/View]
네임스페이스: Project05.UI.View

UI 화면 표시만 담당. World와 App을 직접 참조하지 않고 Presenter가 넘겨준 값만 표시한다. 버튼 입력은 onClick으로 Presenter에 알린다.

들어갈 파일:
- MainMenuView.cs (MonoBehaviour) : [Main] 시작, 설정 버튼, 최고 점수·최고 도달 웨이브 텍스트(TMP).
- SettingsView.cs (MonoBehaviour) : [Main·Game] BGM/SFX 볼륨 슬라이더(0 ~ 1)와 현재 값 글자(0 ~ 100 정수, TMP_Text), 닫기 버튼. 패널 루트를 켜고 끈다.
- LoadingView.cs (MonoBehaviour) : [Main] 로딩 진행률(슬라이더·TMP 텍스트), 시작 버튼. 패널 루트를 켜고 끈다.
- HUDView.cs (MonoBehaviour) : [Game] 스테이지-웨이브, 처치 수 current/max, 준비 시간·보스 안내, 히어로 HP·목숨·부활 대기, 코어 HP, 골드, 레벨·XP, 점수. 텍스트(TMP)와 바(Slider) 모두 선택 연결. 숫자는 SetText(format, 숫자)로 할당 없이 넣는다.
- LevelUpChoiceView.cs (MonoBehaviour) : [Game] 레벨업 3택 1. 버튼 3개와 설명 텍스트(TMP). 항상 켜진 오브젝트에 두고 패널 루트를 켜고 끈다.
- BuildMenuView.cs (MonoBehaviour) : [Game] 공터 건설 목록. 템플릿 칸을 타워 종류 수만큼 복제(판 시작 시 1회), 닫기 버튼. 패널 루트를 켜고 끈다.
- BuildMenuEntryView.cs (MonoBehaviour) : [Game] 건설 목록 한 칸: 아이콘 Image, 건설 Button, 비용 TMP 텍스트(이름 텍스트 선택). 해금된 종류가 없을 때 안내 오브젝트(선택).
- CoreMenuView.cs (MonoBehaviour) : [Game] 코어 UI: 해금 탭 / 타워 탭(탭 버튼 전환), 닫기. 칸은 템플릿 복제(판 시작 시 1회).
- CoreUnlockEntryView.cs (MonoBehaviour) : [Game] 해금 탭 한 칸: 아이콘, 이름, 해금 비용, 해금 버튼, 해금됨 표시.
- CoreTowerEntryView.cs (MonoBehaviour) : [Game] 타워 탭 한 칸(공터): 상태 문구, 업그레이드 버튼·비용, 수리 버튼·비용.
- PauseMenuView.cs (MonoBehaviour) : [Game] 일시정지 메뉴(ESC): 계속, 설정, 메인으로.
- ResultView.cs (MonoBehaviour) : [Game] 점수·도달 웨이브 텍스트(TMP), 재시작, 메인으로. 항상 켜진 오브젝트에 두고 패널 루트를 켜고 끈다.
- CommandRejectedView.cs (MonoBehaviour) : [Game] 명령 거절 안내 문구 (골드 부족 등).

구현하면서 이 목록과 실제 파일을 맞춘다. 컨벤션은 저장소 루트의 CONVENTIONS.md를 따른다.
