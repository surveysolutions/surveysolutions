using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiUtil
{
    public static class Extensions
    {
        /// <summary>
        /// http://blogs.msdn.com/b/pfxteam/archive/2012/03/05/10278165.aspx
        /// </summary>
        public static Task ForEachAsync<T>(this IEnumerable<T> source, int sizeOfpartition, Func<T, Task> action)
        {
            return Task.WhenAll(
                from partition in Partitioner.Create(source).GetPartitions(sizeOfpartition)
                select Task.Run(async delegate
                {
                    using (partition)
                        while (partition.MoveNext())
                            await action(partition.Current);
                }));
        }

        public static TException GetSelfOrInnerAs<TException>(this Exception source)
            where TException : Exception
        {
            if (source is TException)
            {
                return (TException)source;
            }

            while (source.InnerException != null)
            {
                return source.InnerException.GetSelfOrInnerAs<TException>();
            }

            return null;
        }
    }
}