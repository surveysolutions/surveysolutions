using System;

namespace WB.Core.Infrastructure.ReadSide.Repository.Accessors
{
    public static class ReadSideRepositoryReaderExtension
    {
        public static T GetById<T>(this IReadSideRepositoryReader<T> reader, Guid id) where T : class, IReadSideRepositoryEntity
        {
            return reader.GetById(id.ToString());
        }
    }
}