[Authoring/Definition]
네임스페이스: Project05.Authoring.Definition

ScriptableObject 정의. 에셋은 Assets/Data/Definition/에 둔다 (루트만 Assets/Resources/). 로직 값과 View 값(프리팹, VFX, 사운드, 아이콘)을 함께 가지며 베이킹 시 나뉜다. 수치는 모두 여기에 둔다 (하드코딩 금지).

들어갈 파일:
- GameDefinition.cs (ScriptableObject) : 모든 Definition을 가진 루트 + 코어 최대 HP. Assets/Resources/GameDefinition.asset 하나를 Bootstrap이 Resources.Load로 불러온다.
- HeroDefinition.cs (ScriptableObject) : 히어로 수치 + 프리팹.
- EnemyDefinition.cs (ScriptableObject) : 적 수치 + 프리팹, 사망 VFX. 보스도 이 정의를 쓴다.
- TowerDefinition.cs (ScriptableObject) : 타워 한 종류의 이름, 해금·건설·수리 비용, aggroRange, 투사체 속도·반경, 단계별 수치, 프리팹, 아이콘.
- TowerLevelDefinition.cs (Serializable class) : 타워 한 단계의 최대 HP·공격력·사거리·공격 주기 틱·업그레이드 비용.
- WaveRuleDefinition.cs (ScriptableObject) : 웨이브 적 수 규칙, 스폰 간격, 준비 시간, 보스 웨이브.
- DifficultyDefinition.cs (ScriptableObject) : 난이도 증가 계수, 보스 배율.
- LevelUpOptionDefinition.cs (ScriptableObject) : 레벨업 선택지 효과 종류, 증가량, 표시 이름. 에셋은 Data/Definition/LevelUpOption/.
- ProgressionDefinition.cs (ScriptableObject) : 레벨별 필요 XP.
- VisualEffectDefinition.cs (ScriptableObject) : 히어로·적·타워·코어 효과 프리팹, 골드·XP 획득 문구 프리팹과 형식. GameDefinition.visualEffect에 연결. 비운 칸은 효과 없음.
- AudioDefinition.cs (ScriptableObject) : 배경음(메뉴·게임·보스)과 효과음 클립, 믹스 배율, 효과음 동시 재생 수. GameDefinition.audio에 연결. 비운 클립은 소리 없음.

구현하면서 이 목록과 실제 파일을 맞춘다. 컨벤션은 저장소 루트의 CONVENTIONS.md를 따른다.
