using System;

namespace WB.Core.SharedKernels.DataCollection.Repositories
{
    public interface IPlainRepository<T>
    {
        T Get(string id);

        void Store(T interview, string id);

        void Delete(string id);
    }
}