[Component/Input]
네임스페이스: Project05.Component.Input

Update에서 쌓이고 CommandSystem이 FixedTick에서 소비하는 입력 데이터.

들어갈 파일:
- InputBuffer.cs (class) : Command 큐.
- Command.cs (struct) : 명령 종류와 인자 (지면 좌표, 슬롯 인덱스, 선택지 번호 등).
- CommandKind.cs (enum) : MoveTo, ToggleBoard, Fire, BuildTower, UpgradeTower, ChooseLevelUpOption

구현하면서 이 목록과 실제 파일을 맞춘다. 컨벤션은 저장소 루트의 CONVENTIONS.md를 따른다.
