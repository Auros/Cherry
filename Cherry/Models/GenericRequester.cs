using Cherry.Interfaces;

namespace Cherry.Models
{
    internal class GenericRequester : IRequester
    {
        public string ID { get; }
        public bool Elevated { get; }
        public string Username { get; }

        public GenericRequester(string id, bool elevated, string username)
        {
            ID = id;
            Elevated = elevated;
            Username = username;
        }
    }
}