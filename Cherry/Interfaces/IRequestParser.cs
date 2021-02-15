using Cherry.Models;

namespace Cherry.Interfaces
{
    public interface IRequestParser
    {
        ParseResponse<RequestData> Parse(string text);
    }
}