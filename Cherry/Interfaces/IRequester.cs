using Cherry.Models;

namespace Cherry.Interfaces
{
    public interface IRequester
    {
        string ID { get; }
        string Username { get; }
        Power Elevation { get; }
    }
}