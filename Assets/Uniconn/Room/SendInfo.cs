namespace Uniconn
{
    public class SendInfo<T>
    {
        public string methodName;
        public object data;
        public Connector.MethodHandler<T> methodHandler;

        public SendInfo(string methodName, object data, Connector.MethodHandler<T> methodHandler)
        {
            this.methodName = methodName;
            this.data = data;
            this.methodHandler = methodHandler;
        }
    }
}