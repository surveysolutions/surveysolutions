using System;
using System.Linq;

namespace WB.UI.Headquarters.API.Feeds
{
    public static class PagingExtensions
    {
        public static IQueryable<T> SkipFullPages<T>(this IQueryable<T> queryable, int pageSize, int totalRowCount)
        {
            return queryable.Skip(totalRowCount - totalRowCount%pageSize);
        }

        public static IQueryable<T> GetPage<T>(this IQueryable<T> queryable, int pageNumber, int pageSize)
        {
            if (pageNumber < 0)
            {
                throw new ArgumentOutOfRangeException("pageNumber", pageNumber, "Page number starts from 1 and should be positive");
            }

            return queryable.Skip((pageNumber - 1)*pageSize).Take(pageSize);
        }
    }
}