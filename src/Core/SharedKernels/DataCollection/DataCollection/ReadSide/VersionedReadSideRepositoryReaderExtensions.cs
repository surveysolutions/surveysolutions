using System;
using Main.Core.Utility;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

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