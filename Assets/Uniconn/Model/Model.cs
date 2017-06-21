namespace Uniconn
{
    public class Model
    {
        public delegate void ErrorHandler(string errorMsg);
        public delegate void NotExistsHandler();
        public delegate void NotAuthedHandler();
        public delegate void Handler(object data);

        private RoomConnector connector;
        private string boxName;
        private string name;

        private Room room;

        public Model(RoomConnector connector, string boxName, string name)
        {
            this.connector = connector;
            this.boxName = boxName;
            this.name = name;

            room = new Room(connector, boxName, name);
        }

        public string GetName()
        {
            return name;
        }

        public Room GetRoom()
        {
            return room;
        }

        public void Create(object data, ErrorHandler errorHandler, NotAuthedHandler notAuthedHandler, Handler handler)
        {
            room.Send("create", data, (object result) =>
            {
                /*try
                {
                    CreateHandler h = handler;
                    if (h == null)
                    {
                        h = new CreateHandler()
                        {
                        };
                    }

                    if (result.isNull("errorMsg") != true)
                    {
                        h.error(result.getString("errorMsg"));
                    }
                    else if (result.isNull("validErrors") != true)
                    {
                        h.notValid(result.getJSONObject("validErrors"));
                    }
                    else if (result.isNull("isNotAuthed") != true && result.getBoolean("isNotAuthed") == true)
                    {
                        h.notAuthed();
                    }
                    else
                    {
                        h.success(result.getJSONObject("savedData"));
                    }

                }
                catch (JSONException e)
                {
                    e.printStackTrace();
                }*/
            });
        }
    }
}