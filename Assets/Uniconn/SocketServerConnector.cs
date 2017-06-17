using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Uniconn
{
    /// <summary>
    /// SOCKET_SERVER로 만들어진 서버와의 접속 및 통신 작업을 처리하는 클래스
    /// </summary>
    public class SocketServerConnector
    {
        private string host;
        private int port;

        private Connector.ConnectedHandler connectedHandler;
        private Connector.ConnectionFailedHandler connectionFailedHandler;
        private Connector.DisconnectedHandler disconnectedHandler;

        private Dictionary<string, List<object>> methodHandlerMap = new Dictionary<string, List<object>>();

        private int sendKey = 0;

        private Socket socket;
        
        private AsyncCallback receiveCallback;

        public void Disconnect()
        {
            if (socket != null)
            {
                socket.Close();
                socket = null;
            }
        }

        public void Reconnect()
        {
            Disconnect();

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);

            bool connected;
            
            try
            {
                socket.Connect(host, port);

                connected = true;
            }
            catch
            {
                Disconnect();

                connected = false;
            }
            
            if (connected == true)
            {
                connectedHandler();

                Byte[] buffer = new Byte[1024];

                socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, receiveCallback, buffer);
            }
            else
            {
                connectionFailedHandler();
            }
        }

        public void Connect(Connector.ConnectionFailedHandler connectionFailedHandler, Connector.ConnectedHandler connectedHandler, Connector.DisconnectedHandler disconnectedHandler)
        {
            this.connectionFailedHandler = connectionFailedHandler;
            this.connectedHandler = connectedHandler;
            this.disconnectedHandler = disconnectedHandler;

            Reconnect();
        }
        
        public class ReceiveData<T>
        {
            public T data;
        }

        private void RunMethod<T>(Connector.MethodHandler<T> methodHandler, string json)
        {
            methodHandler(JsonUtility.FromJson<ReceiveData<T>>(json).data);
        }

        public SocketServerConnector(string host, int port)
        {
            this.host = host;
            this.port = port;

            StringBuilder stringBuilder = new StringBuilder();

            receiveCallback = (IAsyncResult asyncResult) =>
            {
                Byte[] buffer = asyncResult.AsyncState as Byte[];
                
                int receivedBytes = socket.EndReceive(asyncResult);

                if (receivedBytes > 0)
                {
                    Byte[] realBuffer = new Byte[receivedBytes];
                    Array.Copy(buffer, realBuffer, receivedBytes);

                    stringBuilder.Append(Encoding.UTF8.GetString(realBuffer));

                    stringBuilder.ToString().Contains("\r\n");

                    string receivedStr = stringBuilder.ToString();
                    int index;

                    while ((index = receivedStr.IndexOf("\r\n")) != -1)
                    {
                        string json = receivedStr.Substring(0, index);

                        int methodNameIndex = json.IndexOf("\"methodName\"");
                        for (int i = 0; ; i += 1)
                        {
                            if (json[methodNameIndex + 14 + i] == '"')
                            {
                                string methodName = json.Substring(methodNameIndex + 14, i);

                                if (methodHandlerMap.ContainsKey(methodName) == true)
                                {
                                    List<object> methodHandlers = methodHandlerMap[methodName];

                                    foreach (object methodHandler in methodHandlers)
                                    {
                                        GetType().GetMethod("RunMethod", BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(new Type[] { methodHandler.GetType().GetGenericArguments()[0] }).Invoke(this, new object[] { methodHandler, json });
                                    }
                                }

                                break;
                            }
                        }

                        stringBuilder.Remove(0, index + 2);
                        receivedStr = stringBuilder.ToString();
                    }
                }

                socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, receiveCallback, buffer);
            };
        }

        public bool IsConnected()
        {
            return socket != null;
        }

        public void On<T>(string methodName, Connector.MethodHandler<T> methodHandler)
        {

            List<object> methodHandlers;

            if (methodHandlerMap.ContainsKey(methodName) != true)
            {
                methodHandlerMap[methodName] = methodHandlers = new List<object>();
            }
            else
            {
                methodHandlers = methodHandlerMap[methodName];
            }

            methodHandlers.Add(methodHandler);
        }

        public void Off(string methodName, object methodHandler)
        {

            List<object> methodHandlers = methodHandlerMap[methodName];

            methodHandlers.Remove(methodHandler);

            if (methodHandlers.Count == 0)
            {
                Off(methodName);
            }
        }

        public void Off(String methodName)
        {
            methodHandlerMap.Remove(methodName);
        }

        [Serializable]
        public class SendStringData
        {
            public string methodName;
            public string data;
            public int sendKey;
        }

        public void Send<T>(string methodName, object data, Connector.MethodHandler<T> methodHandler)
        {
            Byte[] buffer;

            if (data is string)
            {
                SendStringData sendData = new SendStringData()
                {
                    methodName = methodName,
                    data = (string)data,
                    sendKey = sendKey
                };

                buffer = Encoding.UTF8.GetBytes(JsonUtility.ToJson(sendData) + "\r\n");
            }
            else
            {
                buffer = Encoding.UTF8.GetBytes("{\"methodName\":\"" + methodName + "\",\"data\":" + JsonUtility.ToJson(data) + ",\"sendKey\":" + sendKey + "}\r\n");
            }

            string callbackName = "__CALLBACK_" + sendKey;

            if (methodHandler != null)
            {
                On<T>(callbackName, (T d) =>
                {
                    methodHandler(d);

                    Off(callbackName);
                });
            }

            sendKey += 1;
            
            socket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, (IAsyncResult asyncResult) =>
            {
                socket.EndSend(asyncResult);
            }, buffer);
        }

        public void Send(string methodName, object data)
        {
            Send<object>(methodName, data, null);
        }
    }
}
