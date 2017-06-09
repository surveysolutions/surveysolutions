using System.Linq;

namespace WB.Core.Infrastructure.Fetching
{
    public interface IFetchRequest<TQueried, TFetch> : IOrderedQueryable<TQueried>
    {
    }
}