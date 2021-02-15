namespace Cherry.Models
{
    public struct ParseResponse<T> where T : class
    {
        public readonly T? Value;
        public readonly string? Error;
    }
}