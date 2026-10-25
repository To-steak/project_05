[Component/Data]
네임스페이스: Project05.Component.Data

ScriptableObject(~Definition)에서 베이킹된 읽기 전용 테이블 struct. 생성 시점에만 조회한다.

들어갈 파일:
- GameData.cs (class) : 베이킹된 테이블 모음 (App.Data의 타입) + 코어 최대 HP. 판 시작 시 World가 참조를 받는다.
- HeroStats.cs (struct) : 최대 HP, 목숨 수, 이동 속도, 근거리 공격력·주기 틱·사거리·부채꼴 각도, 원거리 공격력·주기 틱·사거리, 구체 시작 수·레벨 간격·최대 수·반지름·틱당 각도, 투사체 속도·반경, 접촉 반경, 무적 틱, 부활 틱, 획득 반경, 상호작용 반경.
- EnemyStats.cs (struct) : 기본 HP, 이동 속도, 접촉 반경, 접촉 피해, 코어·타워 공격력, 공격 주기 틱, 기본 드롭 골드·XP, 보스 여부, 처치 점수.
- TowerStats.cs (struct) : 해금 비용, 건설 비용, 수리 비용, aggroRange, 투사체 속도·반경, 탑승 사격 폭발 반경, 단계별 수치(TowerLevelStats 배열).
- TowerLevelStats.cs (struct) : 한 단계의 최대 HP·공격력·사거리·공격 주기 틱, 다음 단계 업그레이드 비용.
- WaveRuleStats.cs (struct) : 웨이브 적 수 계수 (임시: x × 10), 스폰 간격 틱, 준비 시간 틱, 보스 웨이브 번호 (10). 적 구성 생성 규칙은 미정.
- DifficultyStats.cs (struct) : hpGrowth (지수), goldGrowth·xpGrowth (선형), 보스 HP·골드·XP 배율.
- LevelUpOptionStats.cs (struct) : 효과 종류와 증가량 (HP·공격력은 고정값, 획득량은 퍼센트).
- LevelUpEffectKind.cs (enum) : HeroMaxHP, CoreMaxHP, HeroMeleeAttackDamage, GoldGainPercent, XPGainPercent, HeroRangedAttackDamage (에셋에 정수로 저장되므로 순서 유지, 새 값은 끝에 추가)
- ProgressionStats.cs (struct) : 레벨별 필요 XP.

구현하면서 이 목록과 실제 파일을 맞춘다. 컨벤션은 저장소 루트의 CONVENTIONS.md를 따른다.
