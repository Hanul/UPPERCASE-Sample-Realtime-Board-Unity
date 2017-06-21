using System.Collections.Generic;
using UnityEngine;

namespace Uniconn
{
    public class Room
    {
        private RoomConnector connector;

        private string roomName;
        private Dictionary<string, List<object>> methodHandlerMap = new Dictionary<string, List<object>>();
        private bool exited;

        public Room(RoomConnector connector, string boxName, string name)
        {
            this.connector = connector;
            connector.EnterRoom(roomName = boxName + "/" + name);
        }

        public void On<T>(string methodName, Connector.MethodHandler<T> methodHandler)
        {
            List<object> methodHandlers = methodHandlerMap[roomName + "/" + methodName];

            connector.On(roomName + "/" + methodName, methodHandler);

            if (methodHandlerMap[roomName + "/" + methodName] == null)
            {
                methodHandlerMap.Add(roomName + "/" + methodName, methodHandlers = new List<object>());
            }

            methodHandlers.Add(methodHandler);
        }

        public void Off(string methodName, object methodHandler)
        {
            List<object> methodHandlers = methodHandlerMap[roomName + "/" + methodName];

            connector.Off(roomName + "/" + methodName, methodHandler);

            methodHandlers.Remove(methodHandler);

            if (methodHandlers.Count == 0)
            {
                Off(methodName);
            }
        }

        public void Off(string methodName)
        {
            List<object> methodHandlers = methodHandlerMap[roomName + "/" + methodName];

            foreach (object methodHandler in methodHandlers)
            {
                connector.Off(roomName + "/" + methodName, methodHandler);
            }

            methodHandlerMap.Remove(roomName + "/" + methodName);
        }

        public void Send<T>(string methodName, object data, Connector.MethodHandler<T> methodHandler)
        {
            if (exited != true)
            {
                connector.Send(roomName + "/" + methodName, data, methodHandler);
            }
            else
            {
                Debug.LogError("[" + roomName.Replace('/', '.') + "Room.send] 이미 룸에서 나갔습니다,");
            }
        }

        public void Send(string methodName, object data)
        {
            Send<object>(methodName, data, null);
        }

        public void Exit()
        {
            if (exited != true)
            {
                connector.ExitRoom(roomName);

                foreach (string fullMethodName in methodHandlerMap.Keys)
                {
                    List<object> methodHandlers = methodHandlerMap[fullMethodName];

                    foreach (object methodHandler in methodHandlers)
                    {
                        connector.Off(fullMethodName, methodHandler);
                    }

                    methodHandlerMap.Remove(fullMethodName);
                }

                // free method handler map.
                methodHandlerMap = null;

                exited = true;
            }
        }
    }
}