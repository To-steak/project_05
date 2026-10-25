namespace Project05.Foundation
{
    /// <summary>
    /// 배경음 종류. AudioData.musicClips의 인덱스다.
    /// </summary>
    public enum BackgroundMusicKind
    {
        // 배경음 없음 (정지)
        None,
        // Main 씬 메뉴
        MainMenu,
        // Game 씬 일반 진행
        Game,
        // 보스가 살아 있는 동안
        Boss,
    }
}
