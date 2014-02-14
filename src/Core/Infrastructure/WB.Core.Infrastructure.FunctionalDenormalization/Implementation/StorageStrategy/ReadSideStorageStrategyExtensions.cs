using System;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.Infrastructure.FunctionalDenormalization.Implementation.StorageStrategy
{
    public static class ReadSideStorageStrategyExtensions
    {
        public static T Select<T>(this IStorageStrategy<T> storageStrategy, Guid id) where T : class
        {
            return storageStrategy.Select(id.FormatGuid());
        }

        public static void AddOrUpdate<T>(this IStorageStrategy<T> storageStrategy, T projection, Guid id) where T : class
        {
            storageStrategy.AddOrUpdate(projection, id.FormatGuid());
        }

        public static void Delete<T>(this IStorageStrategy<T> storageStrategy, T projection, Guid id) where T : class
        {
            storageStrategy.Delete(projection, id.FormatGuid());
        }
    }
}