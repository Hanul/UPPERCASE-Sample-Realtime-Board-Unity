using System.Collections.Generic;

namespace Uniconn
{
    /// <summary>
    /// UPPERCASE-ROOM 모듈의 룸 서버와의 접속 및 통신 작업을 처리하는 클래스
    /// </summary>
    public class RoomConnector
    {
        private SocketServerConnector connector;

        private List<object> waitingSendInfos = new List<object>();
        private List<string> enterRoomNames = new List<string>();

        public RoomConnector(string host, int port)
        {
            connector = new SocketServerConnector(host, port);
        }

        public void Connect(Connector.ConnectionFailedHandler connectionFailedHandler, Connector.ConnectedHandler connectedHandler, Connector.DisconnectedHandler disconnectedHandler)
        {
            connector.Connect(connectionFailedHandler, () => {

                foreach (string roomName in enterRoomNames)
                {
                    Send("__ENTER_ROOM", roomName);
                }

                foreach (SendInfo<object> sendInfo in waitingSendInfos)
                {
                    Send(sendInfo.methodName, sendInfo.data, sendInfo.methodHandler);
                }

                waitingSendInfos = new List<object>();

                connectedHandler();

            }, disconnectedHandler);
        }

        public void Reconnect()
        {
            connector.Reconnect();
        }

        public void Disconnect()
        {
            connector.Disconnect();
        }

        public bool IsConnected()
        {
            return connector.IsConnected();
        }

        public void On<T>(string methodName, Connector.MethodHandler<T> methodHandler)
        {
            connector.On(methodName, methodHandler);
        }

        public void Off(string methodName, object methodHandler)
        {
            connector.Off(methodName, methodHandler);
        }

        public void Off(string methodName)
        {
            connector.Off(methodName);
        }

        public void Send<T>(string methodName, object data, Connector.MethodHandler<T> methodHandler)
        {
            if (IsConnected() != true)
            {
                waitingSendInfos.Add(new SendInfo<T>(methodName, data, methodHandler));
            }
            else
            {
                connector.Send(methodName, data, methodHandler);
            }
        }

        public void Send(string methodName, object data)
        {
            Send<object>(methodName, data, null);
        }

        public void EnterRoom(string roomName)
        {
            enterRoomNames.Add(roomName);
            if (IsConnected() != true)
            {
                Send("__ENTER_ROOM", roomName);
            }
        }

        public void ExitRoom(string roomName)
        {
            if (IsConnected() != true)
            {
                Send("__EXIT_ROOM", roomName);
            }
            enterRoomNames.Remove(roomName);
        }
    }
}