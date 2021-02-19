using Cherry.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cherry.Interfaces
{
    public interface IRequestHistory
    {
        Task<IEnumerable<CachedRequest>> History();
    }
}
