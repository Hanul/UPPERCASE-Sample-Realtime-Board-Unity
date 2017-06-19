using System.Collections;

namespace Uniconn
{
    /// <summary>
    /// UPPERCASE 서버와의 접속 및 통신 작업을 처리하는 클래스
    /// </summary>
    public class Connector : RoomConnector
    {
        public delegate void ConnectionFailedHandler();
        public delegate void ConnectedHandler();
        public delegate void DisconnectedHandler();
        public delegate void MethodHandler<T>(T data);
        
        public IEnumerator Connect(string doorHost, bool isSecure, int webServerPort, int socketServerPort)
        {
            yield return Request.Get((isSecure ? "https://" : "http://") + doorHost + ":" + webServerPort + "/__SOCKET_SERVER_HOST?defaultHost=" + doorHost, (errorMsg) =>
            {
                connectionFailedHandler();
            }, (host) =>
            {
                Connect(host, socketServerPort);
            });
        }
    }
}