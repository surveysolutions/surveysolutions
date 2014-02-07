using System;

namespace WB.Core.Infrastructure.FunctionalDenormalization.Implementation.StorageStrategy
{
    public static class ReadSideStorageStrategyExtension
    {
        public static T Select<T>(this IStorageStrategy<T> storageStrategy, Guid id) where T : class
        {
            return storageStrategy.Select(id.ToString());
        }

        public static void AddOrUpdate<T>(this IStorageStrategy<T> storageStrategy, T projection, Guid id) where T : class
        {
            storageStrategy.AddOrUpdate(projection, id.ToString());
        }

        public static void Delete<T>(this IStorageStrategy<T> storageStrategy, T projection, Guid id) where T : class
        {
            storageStrategy.Delete(projection, id.ToString());
        }
    }
}