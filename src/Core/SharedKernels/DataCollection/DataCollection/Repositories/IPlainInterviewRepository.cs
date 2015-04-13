using System;

using WB.Core.SharedKernels.DataCollection.Aggregates;

namespace WB.Core.SharedKernels.DataCollection.Repositories
{
    public interface IPlainInterviewRepository<T>
        where T : IInterview
    {
        T GetInterview(Guid id);

        void StoreInterview(T interview, Guid id);

        void DeleteInterview(Guid id);
    }
}