﻿using System;
using System.Linq;
using System.Reflection;
using Ncqrs;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Aggregates;

namespace WB.Core.Infrastructure.Implementation.Aggregates
{
    internal class PlainAggregateRootRepository : IPlainAggregateRootRepository
    {
        private readonly Type genericRepositoryType = typeof(IPlainAggregateRootRepository<>);

        public T Get<T>(Guid aggregateId) where T : class, IPlainAggregateRoot
        {
            return (T) Get(typeof(T), aggregateId);
        }

        public IPlainAggregateRoot Get(Type aggregateRootType, Guid aggregateId)
        {
            Type specificRepositoryType = this.genericRepositoryType.MakeGenericType(aggregateRootType);

            object specificRepository = GetSpecificRepositoryOrThrow(aggregateRootType, specificRepositoryType);

            MethodInfo methodInfo = specificRepositoryType.GetMethod(nameof(IPlainAggregateRootRepository<IPlainAggregateRoot>.Get));

            return methodInfo.Invoke(specificRepository, new object[] { aggregateId }) as IPlainAggregateRoot;
        }

        public void Save(IPlainAggregateRoot aggregateRoot)
        {
            var aggregateRootType = aggregateRoot.GetType();

            Type specificRepositoryType = this.genericRepositoryType.MakeGenericType(aggregateRootType);

            object specificRepository = GetSpecificRepositoryOrThrow(aggregateRootType, specificRepositoryType);

            MethodInfo methodInfo = specificRepositoryType.GetMethod(nameof(IPlainAggregateRootRepository<IPlainAggregateRoot>.Save));

            methodInfo.Invoke(specificRepository, new object[] { aggregateRoot });
        }

        private static object GetSpecificRepositoryOrThrow(Type aggregateRootType, Type specificRepositoryType)
        {
            var repositoryInstances = ServiceLocator.Current.GetAllInstances(specificRepositoryType).ToList();

            if (!repositoryInstances.Any())
                throw new NotSupportedException(
                    $"Aggregate root of type {aggregateRootType.Name} is not supported by plain repository.");

            return repositoryInstances.Single();
        }
    }
}