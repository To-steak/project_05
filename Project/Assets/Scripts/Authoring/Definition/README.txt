[Authoring/Definition]
네임스페이스: Project05.Authoring.Definition

ScriptableObject 정의. 로직 값과 View 값(프리팹, VFX, 사운드)을 함께 가지며 베이킹 시 나뉜다.

들어갈 파일:
- GameDefinition.cs (ScriptableObject) : 모든 Definition 목록을 가진 루트.
- HeroDefinition.cs (ScriptableObject) : 히어로 수치 + 프리팹.
- EnemyDefinition.cs (ScriptableObject) : 적 수치 + 프리팹, 사망 VFX.
- TowerDefinition.cs (ScriptableObject) : 타워 단계별 수치 + 단계별 프리팹.
- WaveDefinition.cs (ScriptableObject) : 웨이브 구성.
- LevelUpOptionDefinition.cs (ScriptableObject) : 레벨업 선택지 수치 + 아이콘, 설명.
- ProgressionDefinition.cs (ScriptableObject) : 레벨별 필요 XP.

구현하면서 이 목록과 실제 파일을 맞춘다. 컨벤션은 저장소 루트의 CONVENTIONS.md를 따른다.
