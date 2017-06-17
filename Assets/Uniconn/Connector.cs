namespace Uniconn
{
    /// <summary>
    /// UPPERCASE 서버와의 접속 및 통신 작업을 처리하는 클래스
    /// </summary>
    public class Connector
    {
        public delegate void ConnectionFailedHandler();
        public delegate void ConnectedHandler();
        public delegate void DisconnectedHandler();
        public delegate void MethodHandler<T>(T data);

        private string doorHost;
        private bool isSecure;
        private int webServerPort;

        private ConnectionFailedHandler connectionFailedHandler;

        private RoomConnector roomConnector;
        
        public Connector(string doorHost, bool isSecure, int webServerPort, int socketServerPort)
        {
            this.doorHost = doorHost;
            this.isSecure = isSecure;
            this.webServerPort = webServerPort;

            Request.Get((isSecure ? "https://" : "http://") + doorHost + ":" + webServerPort + "/__SOCKET_SERVER_HOST?defaultHost=" + doorHost, (errorMsg) =>
            {
                connectionFailedHandler();
            }, (host) =>
            {
                roomConnector = new RoomConnector(host, socketServerPort);
            });
        }

        public void Connect(ConnectionFailedHandler connectionFailedHandler, ConnectedHandler connectedHandler, DisconnectedHandler disconnectedHandler)
        {
            roomConnector.Connect(connectionFailedHandler, connectedHandler, disconnectedHandler);
        }

        public void Reconnect()
        {
            roomConnector.Reconnect();
        }

        public void Disconnect()
        {
            roomConnector.Disconnect();
        }

        public bool IsConnected()
        {
            return roomConnector.IsConnected();
        }

        public void On<T>(string methodName, MethodHandler<T> methodHandler)
        {
            roomConnector.On<T>(methodName, methodHandler);
        }

        public void Off(string methodName, object methodHandler)
        {
            roomConnector.Off(methodName, methodHandler);
        }

        public void Off(string methodName)
        {
            roomConnector.Off(methodName);
        }

        public void Send<T>(string methodName, object data, MethodHandler<T> methodHandler)
        {
            roomConnector.Send<T>(methodName, data, methodHandler);
        }

        public void Send(string methodName, object data)
        {
            roomConnector.Send(methodName, data);
        }

        public void EnterRoom(string roomName)
        {
            roomConnector.EnterRoom(roomName);
        }

        public void ExitRoom(string roomName)
        {
            roomConnector.ExitRoom(roomName);
        }
    }
}