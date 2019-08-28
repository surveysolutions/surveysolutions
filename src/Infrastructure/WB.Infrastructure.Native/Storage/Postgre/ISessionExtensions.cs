using System;
using System.Collections.Concurrent;
using NHibernate;
using NHibernate.Persister.Entity;

namespace WB.Infrastructure.Native.Storage.Postgre
{
    public static class ISessionExtensions
    {
        private static readonly ConcurrentDictionary<Type, string> tableNamesCache = new ConcurrentDictionary<Type, string>();

        public static string GetFullTableName<T>(this ISession session) where T : new()
        {
            return tableNamesCache.GetOrAdd(typeof(T), key =>
            {
                var sessionImplementation = session.GetSessionImplementation();
                var instance = new T();

                var rootTableName = ((ILockable)sessionImplementation.GetEntityPersister(key.Name, instance))
                    .RootTableName;
                return rootTableName;
            });
        }
    }
}
