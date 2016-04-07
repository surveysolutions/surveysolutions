using System;
using System.Linq;
using System.Reflection;
using Microsoft.Practices.ServiceLocation;
using Ncqrs;
using WB.Core.Infrastructure.Aggregates;

namespace WB.Core.Infrastructure.Implementation.Aggregates
{
    internal class PlainAggregateRootRepository : IPlainAggregateRootRepository
    {
        private readonly Type plainAggregateRootRepositoryType = typeof(IPlainAggregateRootRepository<>);

        public T Get<T>(Guid aggregateId) where T : class, IPlainAggregateRoot
        {
            return (T)Get(typeof(T), aggregateId);
        }

        public IPlainAggregateRoot Get(Type aggregateRootType, Guid aggregateId)
        {
            var plainAggregateRootRepositoryGenericType =
                plainAggregateRootRepositoryType.MakeGenericType(aggregateRootType);

            var plainAggregateRootRepository = GetInstanceOfPlainAggregateRootRepositoryOrNull(plainAggregateRootRepositoryGenericType);

            if (plainAggregateRootRepository == null)
                return null;

            MethodInfo methodInfo = plainAggregateRootRepositoryGenericType.GetMethod("Get");

            return methodInfo.Invoke(plainAggregateRootRepository, new object[] { aggregateId }) as IPlainAggregateRoot;
        }

        public void Save(IPlainAggregateRoot aggregateRoot)
        {
            var plainAggregateRootRepositoryGenericType =
                plainAggregateRootRepositoryType.MakeGenericType(aggregateRoot.GetType());

            var plainAggregateRootRepository = GetInstanceOfPlainAggregateRootRepositoryOrNull(plainAggregateRootRepositoryGenericType);

            if(plainAggregateRootRepository==null)
                return;

            MethodInfo methodInfo = plainAggregateRootRepositoryGenericType.GetMethod("Save");

            methodInfo.Invoke(plainAggregateRootRepository, new object[] { aggregateRoot });
        }

        private object GetInstanceOfPlainAggregateRootRepositoryOrNull(Type plainAggregateRootRepositoryGenericType)
        {
            return ServiceLocator.Current.GetAllInstances(plainAggregateRootRepositoryGenericType).SingleOrDefault();
        }
    }
}