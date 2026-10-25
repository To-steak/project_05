namespace Project05.View
{
    /// <summary>
    /// 사망 애니메이션을 재생 중인 적 View (ViewContext.activeEnemyCorpses의 원소).
    /// Logic 슬롯은 이미 비었으므로 EntityID 매핑에서 떼어 두고, 반납 시각이 되면 EnemyViewSystem이 풀에 돌려준다.
    /// </summary>
    public struct ActiveEnemyCorpse
    {
        // 사망 애니메이션 중인 적 View
        public EnemyView enemyView;
        // 풀에 반납할 Time.time (월드가 멈춘 동안은 흐른 시간만큼 미룬다)
        public float releaseTime;
    }
}
