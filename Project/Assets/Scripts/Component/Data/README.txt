[Component/Data]
네임스페이스: Project05.Component.Data

ScriptableObject에서 베이킹된 읽기 전용 테이블 struct.

들어갈 파일:
- GameData.cs (class) : 베이킹된 테이블 모음 (App.Data의 타입).
- HeroStats.cs (struct) : HP, 이동 속도, 근거리·원거리 사거리, 공격 주기, 공격력, 획득 반경, 탑승 반경, 부활 틱.
- EnemyStats.cs (struct) : HP, 이동 속도, 코어 피해량, 드롭 골드·XP.
- TowerStats.cs (struct) : 건설·강화 비용, 단계별 공격력·사거리·공격 주기, 탑승 배율.
- WaveEntry.cs (struct) : 적 종류, 수, 스폰 간격 틱, 스폰 지점 인덱스.
- WaveStats.cs (struct) : 웨이브별 WaveEntry 범위, 준비 시간 틱.
- LevelUpOptionStats.cs (struct) : 선택지 종류와 효과 수치.
- LevelUpEffectKind.cs (enum) : 히어로 강화, 탑승 타워 강화, 특수 능력 등.
- ProgressionStats.cs (struct) : 레벨별 필요 XP.

구현하면서 이 목록과 실제 파일을 맞춘다. 컨벤션은 저장소 루트의 CONVENTIONS.md를 따른다.
