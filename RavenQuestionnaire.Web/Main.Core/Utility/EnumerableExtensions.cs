namespace Main.Core.Utility
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The enumerable extensions.
    /// </summary>
    public static class EnumerableExtensions
    {
        #region Public Methods and Operators

        /// <summary>
        /// Break a list of items into chunks of a specific size
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="chunksize">
        /// The chunksize.
        /// </param>
        /// <returns>
        /// The System.Collections.Generic.IEnumerable`1[T -&gt; System.Collections.Generic.IEnumerable`1[T -&gt; T]].
        /// </returns>
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
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="chunksize">
        /// The chunksize.
        /// </param>
        /// <param name="skipCondition">
        /// The skip Condition.
        /// </param>
        /// <returns>
        /// The System.Collections.Generic.IEnumerable`1[T -&gt; System.Collections.Generic.IEnumerable`1[T -&gt; T]].
        /// </returns>
        public static IEnumerable<IEnumerable<T>> Chunk<T>(
            this IEnumerable<T> source, int chunksize, Func<T, T, bool> skipCondition) where T : class
        {
            while (source.Any())
            {
                T previous = null;
                IEnumerable<T> result = source.TakeWhile(
                    (x, i) =>
                        {
                            bool isSkip = i < chunksize || (i >= chunksize && skipCondition(x, previous));
                            previous = x;
                            return isSkip;
                        });
                yield return result;

                // chunksize = chunksize + justSkipped;
                source = source.Skip(result.Count());
            }
        }

        /// <summary>
        /// Break a list of items into chunks of a specific size
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="chunksize">
        /// The chunksize.
        /// </param>
        /// <param name="skipCondition">
        /// The skip Condition.
        /// </param>
        /// <returns>
        /// The System.Collections.Generic.IEnumerable`1[T -&gt; System.Collections.Generic.IEnumerable`1[T -&gt; T]].
        /// </returns>
        public static IEnumerable<IEnumerable<T>> Chunk<T>(
            this IEnumerable<T> source, int chunksize, Func<T, bool> skipCondition)
        {
            while (source.Any())
            {
                yield return source.TakeWhile((x, i) => i < chunksize || (i >= chunksize && skipCondition(x)));
                source = source.SkipWhile((x, i) => i < chunksize || (i >= chunksize && skipCondition(x)));
            }
        }

        #endregion
    }
}