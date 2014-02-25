using System;
using WB.Core.GenericSubdomains.Utils;

namespace WB.Core.SharedKernels.DataCollection.ReadSide
{
    public static class VersionedReadSideRepositoryReaderExtensions
    {
        public static T GetById<T>(this IVersionedReadSideRepositoryReader<T> reader, Guid id, long version) where T : class, IVersionedView
        {
            return reader.GetById(id.FormatGuid(), version);
        }
    }
}