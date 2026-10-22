namespace Project05.View
{
    /// <summary>
    /// 한 판의 화면 상태. EntityID → View 오브젝트 매핑, 풀 참조, 씬의 표현용 참조(카메라, 코어 View 등)를 담는다.
    /// Runner가 Game 씬 진입 시 만들고 퇴장 시 버린다. ViewSystem은 static 필드 대신 이 인스턴스만 쓴다.
    /// 매핑과 참조는 각 ViewSystem을 구현하는 기능 브랜치에서 추가한다.
    /// </summary>
    public class ViewContext
    {
    }
}
