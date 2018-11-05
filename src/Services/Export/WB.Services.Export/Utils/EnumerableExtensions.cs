using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace WB.Services.Export.Utils
{
    public static class CommonExtensions
    {
        [DebuggerStepThrough]
        public static string FormatGuid(this Guid guid)
        {
            return guid.ToString("N");
        }
    }

    public static class EnumerableExtensions
    {
        /// <summary>
        /// Batches the source sequence into sized buckets.
        /// </summary>
        /// <typeparam name="TSource">Type of elements in <paramref name="source"/> sequence.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="size">Size of buckets.</param>
        /// <returns>A sequence of equally sized buckets containing elements of the source collection.</returns>
        /// <remarks>
        /// This operator uses deferred execution and streams its results (buckets and bucket content). 
        /// </remarks>
        public static IEnumerable<IEnumerable<TSource>> Batch<TSource>(this IEnumerable<TSource> source, int size)
        {
            return Batch(source, size, x => x);
        }

        /// <summary>
        /// Batches the source sequence into sized buckets and applies a projection to each bucket.
        /// </summary>
        /// <typeparam name="TSource">Type of elements in <paramref name="source"/> sequence.</typeparam>
        /// <typeparam name="TResult">Type of result returned by <paramref name="resultSelector"/>.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="size">Size of buckets.</param>
        /// <param name="resultSelector">The projection to apply to each bucket.</param>
        /// <returns>A sequence of projections on equally sized buckets containing elements of the source collection.</returns>
        /// <remarks>
        /// This operator uses deferred execution and streams its results (buckets and bucket content).
        /// </remarks>
        public static IEnumerable<TResult> Batch<TSource, TResult>(this IEnumerable<TSource> source, int size,
            Func<IEnumerable<TSource>, TResult> resultSelector)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (size <= 0) throw new ArgumentOutOfRangeException(nameof(size));
            if (resultSelector == null) throw new ArgumentNullException(nameof(resultSelector));
            return BatchImpl(source, size, resultSelector);
        }

        private static IEnumerable<TResult> BatchImpl<TSource, TResult>(this IEnumerable<TSource> source, int size,
            Func<IEnumerable<TSource>, TResult> resultSelector)
        {
            TSource[] bucket = null;
            var count = 0;

            foreach (var item in source)
            {
                if (bucket == null)
                {
                    bucket = new TSource[size];
                }

                bucket[count++] = item;

                // The bucket is fully buffered before it's yielded
                if (count != size)
                {
                    continue;
                }

                // Select is necessary so bucket contents are streamed too
                yield return resultSelector(bucket.Select(x => x));

                bucket = null;
                count = 0;
            }

            // Return the last bucket with all remaining elements
            if (bucket != null && count > 0)
            {
                yield return resultSelector(bucket.Take(count));
            }
        }

        public static IEnumerable<List<T>> BatchInTime<T>(this ICollection<T> source, BatchOptions options,
            ILogger logger = null)
        {
            long index = 0;

            // take initial batch around min value
            int batchSize = options.Min;

            using (var queue = source.GetEnumerator())
            {
                Stopwatch sw = null;

                while (index < source.Count)
                {
                    var interviewsLeft = source.Count - index;

                    var take = Math.Min(batchSize, (int) interviewsLeft);

                    if (sw != null)
                    {
                        var elapsed = sw.Elapsed.TotalSeconds;
                        var oldBatchSize = batchSize;
                        logger.LogTrace($"Took {sw.Elapsed.TotalSeconds:F} seconds to process {batchSize} items");

                        // delta need just to make batch size to end up in some stable position, and don't change every iteration
                        if (elapsed - options.TargetSeconds > options.TargetSeconds * options.TargetDelta)
                        {
                            batchSize = batchSize - (int) (batchSize * options.TargetStep);
                            if (batchSize == oldBatchSize) batchSize -= 1;
                            batchSize = Math.Max(options.Min, batchSize);

                            logger?.LogDebug($"Changed batch size to {batchSize}. Too slow execution ({elapsed - options.TargetSeconds:F}s)");
                        }
                        else if (elapsed - options.TargetSeconds < -options.TargetSeconds * options.TargetDelta)
                        {
                            batchSize = batchSize + (int) (batchSize * options.TargetStep);
                            if (batchSize == oldBatchSize) batchSize += 1;
                            batchSize = Math.Min(batchSize, options.Max);

                            logger?.LogDebug($"Changed batch size to {batchSize}. Can go faster ({elapsed - options.TargetSeconds:F}s)");
                        }
                        
                        sw.Restart();
                    }
                    else
                    {
                        sw = Stopwatch.StartNew();
                    }

                    var result = new List<T>();

                    for (int i = 0; i < take; i++)
                    {
                        if (!queue.MoveNext()) break;
                        result.Add(queue.Current);
                        index++;
                    }

                    yield return result;
                }
                
            }
        }
    }

    public class BatchOptions
    {
        public double TargetSeconds { get; set; } = 2;
        public double TargetDelta { get; set; } = 0.2;
        public double TargetStep { get; set; } = 0.25;
        public int Max { get; set; }
        public int Min { get; set; } = 1;
    }
}
