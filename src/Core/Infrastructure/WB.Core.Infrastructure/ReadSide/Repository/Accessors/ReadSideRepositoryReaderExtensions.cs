using System;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.Infrastructure.ReadSide.Repository.Accessors
{
    public static class ReadSideRepositoryReaderExtensions
    {
        public static T GetById<T>(this IReadSideRepositoryReader<T> reader, Guid id) where T : class, IReadSideRepositoryEntity
        {
            return reader.GetById(id.FormatGuid());
        }
    }
}