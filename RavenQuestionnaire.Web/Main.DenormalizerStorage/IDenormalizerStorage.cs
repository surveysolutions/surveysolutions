using System;
using System.Linq;

namespace Main.DenormalizerStorage
{
    public interface IDenormalizerStorage<T>
        where T : class
    {
        int Count();

        T GetByGuid(Guid key);

        IQueryable<T> Query();

        void Remove(Guid key);

        void Store(T denormalizer, Guid key);
    }
}