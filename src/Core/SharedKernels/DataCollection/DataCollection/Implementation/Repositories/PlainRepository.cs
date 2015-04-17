using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Repositories
{
    internal class PlainRepository<T> : IPlainRepository<T> where T : class
    {
        private readonly IPlainStorageAccessor<T> repository;

        public PlainRepository(IPlainStorageAccessor<T> repository)
        {
            this.repository = repository;
        }

        public T Get(string id)
        {
            return this.repository.GetById(id);
        }

        public void Store(T entity, string id)
        {
            this.repository.Store(entity, id);
        }

        public void Delete(string id)
        {
            this.repository.Remove(id);
        }
    }
}