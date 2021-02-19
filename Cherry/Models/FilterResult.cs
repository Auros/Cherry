namespace Cherry.Models
{
    public class FilterResult
    {
        public bool IsValid { get; }
        public string? Error { get; }

        public FilterResult(bool isValid, string? error = null)
        {
            IsValid = isValid;
            Error = error;
        }
    }
}