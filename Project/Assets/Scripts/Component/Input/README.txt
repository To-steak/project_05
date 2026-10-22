[Component/Input]
네임스페이스: Project05.Component.Input

Update에서 쌓이고 CommandSystem이 FixedTick에서 소비하는 입력 데이터.

들어갈 파일:
- InputBuffer.cs (class) : Command 큐.
- Command.cs (struct) : 명령 종류와 인자 (지면 좌표, 공터·타워 인덱스, 타워 종류, 선택지 번호 등).
- InputBuffer.cs (class) : 명령 큐 + 좌클릭을 누르고 있는 상태와 커서 지면 좌표(연속 입력: 누르고 있으면 지상 이동 추적, 탑승 중 쿨타임마다 사격).
- CommandKind.cs (enum) : MoveTo, Interact (Space: 건설 목록 / 탑승 / 하차 / 코어 UI 열기), BuildTower, CloseBuildMenu, ChooseLevelUpOption, UnlockTowerType, UpgradeTower, RepairTower, CloseCoreMenu, DebugClearWave (디버그 패널 전용: 웨이브 즉시 클리어)

구현하면서 이 목록과 실제 파일을 맞춘다. 컨벤션은 저장소 루트의 CONVENTIONS.md를 따른다.
