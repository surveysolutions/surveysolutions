using System;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Repositories
{
    internal class PlainInterviewRepository<T> : IPlainInterviewRepository<T>
        where T : class, IInterview
    {
        private readonly IPlainStorageAccessor<T> repository;

        public PlainInterviewRepository(IPlainStorageAccessor<T> repository)
        {
            this.repository = repository;
        }

        public T GetInterview(Guid id)
        {
            return this.repository.GetById(id.FormatGuid());
        }

        public void StoreInterview(T interview, Guid id)
        {
            this.repository.Store(interview, id.FormatGuid());
        }

        public void DeleteInterview(Guid id)
        {
            this.repository.Remove(id.FormatGuid());
        }
    }
}