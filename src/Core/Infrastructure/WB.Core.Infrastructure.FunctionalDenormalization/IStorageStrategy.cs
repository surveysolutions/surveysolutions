using System;

namespace WB.Core.Infrastructure.FunctionalDenormalization
{
    public interface IStorageStrategy<T> where T : class
    {
        T Select(string id);
        void AddOrUpdate(T projection, string id);
        void Delete(T projection, string id);
    }
}