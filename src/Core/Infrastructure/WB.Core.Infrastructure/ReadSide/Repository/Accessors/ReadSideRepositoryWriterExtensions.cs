using System;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.Infrastructure.ReadSide.Repository.Accessors
{
    public static class ReadSideRepositoryWriterExtensions
    {
        public static T GetById<T>(this IReadSideRepositoryWriter<T> writer, Guid id) where T : class, IReadSideRepositoryEntity
        {
            return writer.GetById(id.FormatGuid());
        }

        public static void Remove<T>(this IReadSideRepositoryWriter<T> writer, Guid id) where T : class, IReadSideRepositoryEntity
        {
            writer.Remove(id.FormatGuid());
        }

        public static void Store<T>(this IReadSideRepositoryWriter<T> writer, T view, Guid id) where T : class, IReadSideRepositoryEntity
        {
            writer.Store(view, id.FormatGuid());
        }
    }
}