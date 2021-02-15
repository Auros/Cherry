namespace Cherry.Interfaces
{
    public interface IRequester
    {
        string ID { get; }
        bool Elevated { get; }
        string Username { get; }
    }
}