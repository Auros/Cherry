using Cherry.Models;
using System.Threading.Tasks;

namespace Cherry.Interfaces
{
    public interface IRequestFilter<T> where T : notnull
    {
        Task<FilterResult> Resolve(T subject, RequestEventArgs requestData);
    }
}