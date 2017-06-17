namespace Uniconn.Handler
{
    /// <summary>
    /// 서버와의 접속이 실패했을 때 실행되는 핸들러를 작성하기 위한 인터페이스
    /// </summary>
    public interface ConnectionFailedHandler
    {
        void Handle();
    }
}
