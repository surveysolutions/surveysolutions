using System;

namespace WB.Core.Infrastructure.FunctionalDenormalization.Implementation.StorageStrategy
{
    public class SingleEventSourceStorageStrategy<T> : IStorageStrategy<T> where T : class
    {
        private  T view;
        public SingleEventSourceStorageStrategy(T view)
        {
            this.view = view;
        }

        public T Select(Guid id)
        {
            return this.view;
        }

        public void AddOrUpdate(T projection, Guid id)
        {
            this.view = projection;
        }

        public void Delete(T projection, Guid id)
        {
            this.view = null;
        }
    }
}
