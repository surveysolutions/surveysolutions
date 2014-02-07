using System;

namespace WB.Core.SharedKernels.DataCollection.ReadSide
{
    public static class VersionedReadSideRepositoryReaderExtension
    {
        public static T GetById<T>(this IVersionedReadSideRepositoryReader<T> reader, Guid id, long version) where T : class, IVersionedView
        {
            return reader.GetById(id.ToString(), version);
        }
    }
}