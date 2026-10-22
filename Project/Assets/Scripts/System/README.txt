[System]
네임스페이스: Project05.System

상태 없는 static 로직. 시그니처 `public static void Tick(World world)`. World만 본다 (App 사용 금지). 어셈블리 참조: Component. 일시정지 중에는 CommandSystem만 실행, Defeat에서는 전부 정지.

들어갈 파일:
- CommandSystem.cs (static class) : [Tick 1] 입력 버퍼 소비. Interact(Space)는 히어로 위치 기준으로 대상(빈 공터 / 타워 / 코어)을 찾아 건설 요청·탑승·하차·코어 UI 열기로 해석. 일시정지 중에는 열린 UI의 명령만 처리.
- GameFlowSystem.cs (static class) : [Tick 2] 준비 / 웨이브 / 보스 / 패배 전이. 준비 시간 후 웨이브 시작, ClearWave(다음 웨이브 준비), 누적 웨이브 번호 계산. 처치 수가 max에 도달하면 웨이브 클리어 또는 보스 생성 요청. 보스 처치 시 스테이지 클리어. 패배 판정 (코어 HP 0, 히어로 목숨 0).
- WaveSystem.cs (static class) : [Tick 3] 스폰 간격마다 적 생성 요청 (총 max = x × 10). 임시 규칙: 일반 적(보스 아닌 첫 종류)만, 스폰 지점 순서대로.
- HeroSystem.cs (static class) : [Tick 4] 이동, 공격 타이머 1개로 근거리 광역 / 원거리 자동 공격, 접촉 피해와 무적, 사망·목숨·부활.
- TowerSystem.cs (static class) : [Tick 5] 미탑승 자동 공격, 탑승 시 클릭 사격 (공격력 = 히어로 공격력). 업그레이드·수리 처리 함수 제공.
- ProjectileSystem.cs (static class) : [Tick 5.5] 직선 투사체 이동, 이동 구간의 첫 적 하나에 명중(피해 후 소멸), 최대 비행 거리 소진 시 소멸. 히어로 원거리·이후 타워 공용.
- EnemySystem.cs (static class) : [Tick 6] 경로 이동, aggroRange 내 타워 우선 공격, 코어 포위·공격, 사망 처리(드롭 생성 요청, 처치 수 증가), 타워 파괴(공터 비움, 탑승 중이면 강제 하차).
- DropSystem.cs (static class) : [Tick 7] 획득 반경 내 드롭 획득 (탑승 중 불가), 획득 퍼센트 적용.
- CoreSystem.cs (static class) : [Tick 8] 코어 피해 처리.
- SpawnSystem.cs (static class) : [Tick 9] 생성 요청 큐 처리 (적·보스·투사체·드롭·타워). 테이블 조회, 난이도 수식 계산, 엔티티에 값 복사. 용량 초과 요청은 다음 틱에 재시도.
- EconomySystem.cs (static class) : [Tick 없음] 골드 증감, 해금·건설·업그레이드·수리 비용 지불.
- ProgressSystem.cs (static class) : [Tick 없음] XP 증가, 레벨업(일시정지), 선택지 3개 뽑기, 효과 적용 (HP 선택 시 전부 회복).
- TargetingSystem.cs (static class) : [Tick 없음] 반경 내 최근접 적, 반경 내 모든 적, 이동 구간에 처음 닿는 적 조회 (히어로·타워·투사체 공용).
- SessionSystem.cs (static class) : [Tick 없음] 새 World 초기화 (저장소 생성, 히어로·코어 설정, 스테이지 1-1). 실행 여부 판단(패배·일시정지), 틱 수 증가, 프레임 기록 비우기(Runner가 LateUpdate 마지막에 호출).

구현하면서 이 목록과 실제 파일을 맞춘다. 컨벤션은 저장소 루트의 CONVENTIONS.md를 따른다.
