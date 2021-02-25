using Cherry.Interfaces;

namespace Cherry.Models
{
    public class GenericRequester : IRequester
    {
        public string ID { get; }
        public string Username { get; }
        public Power Elevation { get; }

        public GenericRequester(string id, string username, Power elevation)
        {
            ID = id;
            Username = username;
            Elevation = elevation;
        }
    }
}