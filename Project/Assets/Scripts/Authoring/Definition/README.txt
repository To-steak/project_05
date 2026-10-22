[Authoring/Definition]
네임스페이스: Project05.Authoring.Definition

ScriptableObject 정의. 로직 값과 View 값(프리팹, VFX, 사운드, 아이콘)을 함께 가지며 베이킹 시 나뉜다. 수치는 모두 여기에 둔다 (하드코딩 금지).

들어갈 파일:
- GameDefinition.cs (ScriptableObject) : 모든 Definition을 가진 루트. 에셋은 Assets/Resources/GameDefinition.asset (DefinitionBaker가 Resources.Load). 개별 Definition 에셋은 Assets/Data/Definition/.
- HeroDefinition.cs (ScriptableObject) : 히어로 수치 + 프리팹.
- EnemyDefinition.cs (ScriptableObject) : 적 수치 + 프리팹, 사망 VFX. 보스도 이 정의를 쓴다.
- TowerDefinition.cs (ScriptableObject) : 타워 종류별 비용, 단계별 수치 + 단계별 프리팹, 아이콘.
- WaveRuleDefinition.cs (ScriptableObject) : 웨이브 적 수 규칙, 스폰 간격, 준비 시간, 보스 웨이브.
- DifficultyDefinition.cs (ScriptableObject) : 난이도 증가 계수, 보스 배율.
- LevelUpOptionDefinition.cs (ScriptableObject) : 레벨업 선택지 효과와 증가량 + 아이콘, 설명.
- ProgressionDefinition.cs (ScriptableObject) : 레벨별 필요 XP.

구현하면서 이 목록과 실제 파일을 맞춘다. 컨벤션은 저장소 루트의 CONVENTIONS.md를 따른다.
