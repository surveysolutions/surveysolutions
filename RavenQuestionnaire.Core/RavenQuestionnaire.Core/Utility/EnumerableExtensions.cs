using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RavenQuestionnaire.Core.Utility
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Break a list of items into chunks of a specific size
        /// </summary>
        public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> source, int chunksize)
        {
            while (source.Any())
            {
                yield return source.Take(chunksize);
                source = source.Skip(chunksize);
            }
        }
        /// <summary>
        /// Break a list of items into chunks of a specific size
        /// </summary>
        public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> source, int chunksize, Func<T,T,  bool> skipCondition) where T:class 
        {

            while (source.Any())
            {
                T previous = null;
                var result = source.TakeWhile((x, i) =>
                                                  {
                                                      var isSkip= i < chunksize ||
                                                             (i >= chunksize && skipCondition(x, previous));
                                                      previous = x;
                                                      return isSkip;
                                                  });
                yield return result;
                //   chunksize = chunksize + justSkipped;
                source = source.Skip(result.Count());
            }
            
        }
        /// <summary>
        /// Break a list of items into chunks of a specific size
        /// </summary>
        public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> source, int chunksize, Func<T, bool> skipCondition)
        {
            while (source.Any())
            {
                yield return source.TakeWhile((x, i) => i < chunksize || (i >= chunksize && skipCondition(x)));
                source = source.SkipWhile((x, i) => i < chunksize || (i >= chunksize && skipCondition(x)));
            }
        }
    }
}
