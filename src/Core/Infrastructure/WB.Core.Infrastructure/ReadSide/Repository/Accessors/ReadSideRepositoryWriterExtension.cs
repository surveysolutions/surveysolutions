using System;

namespace WB.Core.Infrastructure.ReadSide.Repository.Accessors
{
    public static class ReadSideRepositoryWriterExtension
    {
        public static T GetById<T>(this IReadSideRepositoryWriter<T> writer, Guid id) where T : class, IReadSideRepositoryEntity
        {
            return writer.GetById(id.ToString());
        }

        public static void Remove<T>(this IReadSideRepositoryWriter<T> writer, Guid id) where T : class, IReadSideRepositoryEntity
        {
            writer.Remove(id.ToString());
        }

        public static void Store<T>(this IReadSideRepositoryWriter<T> writer, T view, Guid id) where T : class, IReadSideRepositoryEntity
        {
            writer.Store(view, id.ToString());
        }
    }
}