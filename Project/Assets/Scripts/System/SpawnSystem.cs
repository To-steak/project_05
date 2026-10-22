using Project05.Component;

namespace Project05.System
{
    /// <summary>
    /// [Tick 9] 생성 요청 큐를 처리해 적·보스·드롭·타워를 실제로 만든다. 테이블 조회, 난이도 수식 계산, 엔티티 값 복사를 여기서 한다. 용량이 차서 실패한 요청은 다음 틱에 다시 시도한다.
    /// 구현 브랜치: feat/enemy-wave, feat/drop-progress, feat/tower
    /// </summary>
    public static class SpawnSystem
    {
        /// <summary>
        /// 생성 요청 큐를 처리한다. 틱의 마지막에 실행된다.
        /// </summary>
        /// <param name="world">현재 판의 월드</param>
        public static void Tick(World world)
        {
        }
    }
}
