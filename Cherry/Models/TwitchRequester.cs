using Cherry.Interfaces;

namespace Cherry.Models
{
    internal class TwitchRequester : IRequester
    {
        public string ID { get; }
        public string Username { get; }
        public Power Elevation { get; }
    
        public TwitchRequester(string id, string username, Power elevation)
        {
            ID = id;
            Username = username;
            Elevation = elevation;
        }
    }
}