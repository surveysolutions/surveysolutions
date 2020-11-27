using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Domain;
using WB.Core.Infrastructure.EventBus;

namespace WB.Core.Infrastructure
{
    public static class Extensions
    {
        public static void ExecuteInScope<T>(this IServiceLocator sl, Action<T> action)
        {
            sl.GetInstance<IInScopeExecutor>().Execute(scope => action(scope.GetInstance<T>()));
        }   
        
        public static void ExecuteInScope<T>(this IServiceProvider sl, Action<T> action)
        {
            sl.GetService<IInScopeExecutor>().Execute(scope => action(scope.GetInstance<T>()));
        }        
        
        public static TR ExecuteInScope<T, TR>(this IServiceLocator sl, Func<T, TR> action)
        {
            return sl.GetInstance<IInScopeExecutor>().Execute(scope => action(scope.GetInstance<T>()));
        }

        /// <summary>
        /// Override over GetOrCreate that will handle null result from cache entry factory, and not store it
        /// </summary>
        /// <returns>Cached item or non cached null</returns>
        public static TItem GetOrCreateNullSafe<TItem>(this IMemoryCache cache, object key, Func<ICacheEntry, TItem> factory)
            where TItem : class
        {
            if (!cache.TryGetValue(key, out object result))
            {
                var entry = cache.CreateEntry(key);
                result = factory(entry);

                if (result == null)
                {
                    return null;
                }

                entry.SetValue(result);
                // need to manually call dispose instead of having a using
                // in case the factory passed in throws, in which case we
                // do not want to add the entry to the cache
                entry.Dispose();
            }

            return (TItem)result;
        }


        /// <summary>
        /// Events created during prototype phase are marked with 'prototype' origin
        /// </summary>
        public static bool IsPrototype<T>(this IPublishedEvent<T> @event) where T : IEvent
        {
            return string.Equals(@event.Origin, "prototype", StringComparison.OrdinalIgnoreCase);
        }
    }
}
