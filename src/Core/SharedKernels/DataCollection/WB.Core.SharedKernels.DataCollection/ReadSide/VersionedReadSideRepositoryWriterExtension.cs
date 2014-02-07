using System;

namespace WB.Core.SharedKernels.DataCollection.ReadSide
{
    public static class VersionedReadSideRepositoryWriterExtension
    {
        public static T GetById<T>(this IVersionedReadSideRepositoryWriter<T> writer, Guid id, long version) where T : class, IVersionedView
        {
            return writer.GetById(id.ToString(), version);
        }

        public static void Remove<T>(this IVersionedReadSideRepositoryWriter<T> writer, Guid id, long version) where T : class, IVersionedView
        {
            writer.Remove(id.ToString(), version);
        }
    }
}