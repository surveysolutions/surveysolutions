using System.Linq;
using System.Reflection;

namespace WB.Core.Infrastructure.ReadSide.Repository.Accessors
{
    public interface IReadSideRepositoryIndexAccessor
    {
        IQueryable<TResult> Query<TResult>(string indexName);
        void RegisterIndexesFormAssembly(Assembly assembly);
    }
}
