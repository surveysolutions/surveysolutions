using System;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.Infrastructure.ReadSide.Repository.Accessors
{
    public static class ReadSideExtensions
    {
        public static ReadSideStorageVersionedWrapper<T> AsVersioned<T>(this IReadSideStorage<T> storage)
            where T : class, IReadSideRepositoryEntity
        {
            return new ReadSideStorageVersionedWrapper<T>(storage);
        }

        public static ReadSideRepositoryReaderVersionedWrapper<T> AsVersioned<T>(this IReadSideRepositoryReader<T> storage)
            where T : class, IReadSideRepositoryEntity
        {
            return new ReadSideRepositoryReaderVersionedWrapper<T>(storage);
        }

        public static T GetById<T>(this IReadSideStorage<T> writer, Guid? id)
            where T : class, IReadSideRepositoryEntity
        {
            return writer.GetById(id.FormatGuid());
        }

        public static void Remove<T>(this IReadSideStorage<T> writer, Guid? id)
            where T : class, IReadSideRepositoryEntity
        {
            writer.Remove(id.FormatGuid());
        }

        public static void Store<T>(this IReadSideStorage<T> writer, T view, Guid? id)
            where T : class, IReadSideRepositoryEntity
        {
            writer.Store(view, id.FormatGuid());
        }

        public static T GetById<T>(this IReadSideRepositoryReader<T> reader, Guid id)
            where T : class, IReadSideRepositoryEntity
        {
            return reader.GetById(id.FormatGuid());
        }
    }
}