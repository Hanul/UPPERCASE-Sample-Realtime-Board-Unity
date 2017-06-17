namespace Uniconn
{
    public class Model
    {
        public delegate void ErrorHandler(string errorMsg);

        public delegate void NotExistsHandler();

        public delegate void NotAuthedHandler();

        public delegate void Handler(object data);
    }
}