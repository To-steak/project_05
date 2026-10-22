[System]
네임스페이스: Project05.System

상태 없는 static 로직. 시그니처 `public static void Tick(World world)`. World만 본다. 어셈블리 참조: Component.

들어갈 파일:
- CommandSystem.cs (static class) : [Tick 1] 입력 버퍼를 꺼내 각 System의 명령 처리 함수 호출.
- GameFlowSystem.cs (static class) : [Tick 2] 준비 / 웨이브 / 승리 / 패배 전이.
- WaveSystem.cs (static class) : [Tick 3] 웨이브 스케줄, 적 스폰 요청.
- HeroSystem.cs (static class) : [Tick 4] 이동, 탑승/하차, 근거리·원거리 자동 공격, 사망/부활.
- TowerSystem.cs (static class) : [Tick 5] 미탑승 자동 공격, 탑승 사격, 건설, 강화.
- EnemySystem.cs (static class) : [Tick 6] 스폰, 이동, 피해, 사망, 코어 도달.
- DropSystem.cs (static class) : [Tick 7] 드롭 생성, 획득 판정.
- CoreSystem.cs (static class) : [Tick 8] 코어 피해, 패배 판정.
- EconomySystem.cs (static class) : [Tick 없음] 골드 증감, 비용 지불.
- ProgressSystem.cs (static class) : [Tick 없음] XP 증가, 레벨업, 선택지 생성·적용.
- TargetingSystem.cs (static class) : [Tick 없음] 반경 내 최근접 적, 반경 내 모든 적 조회 (히어로·타워 공용).
- SessionSystem.cs (static class) : [Tick 없음] 베이킹된 데이터로 새 World 초기화.

구현하면서 이 목록과 실제 파일을 맞춘다. 컨벤션은 저장소 루트의 CONVENTIONS.md를 따른다.
