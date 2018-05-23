using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.Infrastructure.ReadSide.Repository.Accessors
{
    public static class ReadSideExtensions
    {
        public static T GetById<T>(this IReadSideStorage<T> writer, Guid? id)
            where T : class, IReadSideRepositoryEntity
        {
            return writer.GetById(id.FormatGuid());
        }

        public static T GetById<T>(this IReadSideStorage<T, string> writer, Guid? id)
            where T : class, IReadSideRepositoryEntity
        {
            return writer.GetById(id.FormatGuid());
        }

        public static void Remove<T>(this IReadSideStorage<T> writer, Guid? id)
            where T : class, IReadSideRepositoryEntity
        {
            writer.Remove(id.FormatGuid());
        }

        public static void Remove<T>(this IReadSideStorage<T, string> writer, Guid? id)
            where T : class, IReadSideRepositoryEntity
        {
            writer.Remove(id.FormatGuid());
        }


        public static void Store<T>(this IReadSideStorage<T> writer, T view, Guid? id)
            where T : class, IReadSideRepositoryEntity
        {
            writer.Store(view, id.FormatGuid());
        }

        public static void Store<T>(this IReadSideStorage<T, string> writer, T view, Guid? id)
            where T : class, IReadSideRepositoryEntity
        {
            writer.Store(view, id.FormatGuid());
        }

        public static T GetById<T>(this IReadSideRepositoryReader<T> reader, Guid id)
            where T : class, IReadSideRepositoryEntity
        {
            return reader.GetById(id.FormatGuid());
        }

        public static T GetById<T>(this IReadSideRepositoryReader<T, string> reader, Guid id)
            where T : class, IReadSideRepositoryEntity
        {
            return reader.GetById(id.FormatGuid());
        }

        public static IList<T> QueryAll<T>(this IQueryable<T> reader)
        {
            var result = new List<T>();
            int skipResults = 0;
            while (true)
            {
                var chunk = reader.Skip(skipResults).ToList();

                if (!chunk.Any())
                    break;

                result.AddRange(chunk);
                skipResults = result.Count;
            }
            return result;
        }

        public static void Store<TEntity, TKey>(this IReadSideRepositoryWriter<TEntity, TKey> repo, TEntity entity)
            where TEntity : class, IReadSideRepositoryEntity
        {
            repo.Store(entity, default(TKey));
        }
    }
}
