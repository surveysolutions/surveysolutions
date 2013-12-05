using System;

namespace WB.Core.Infrastructure.FunctionalDenormalization
{
    public interface IStorageStrategy<T> where T : class
    {
        T Select(Guid id);
        void AddOrUpdate(T projection, Guid id);
        void Delete(T projection, Guid id);
    }
}