using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.Headquarters.Storage;

public static class ExternalFileStorageExtensions
{
    // prefixes are removed with bounded concurrency to keep the request rate to the storage under control
    private const int MaxParallelPrefixDeletions = 8;

    /// <summary>
    /// There are no directories in S3, so all objects stored under the prefix
    /// have to be listed and removed by their own keys.
    /// </summary>
    public static async Task RemoveAllUnderPrefixAsync(this IExternalFileStorage externalFileStorage, string prefix)
    {
        while (true)
        {
            var files = await externalFileStorage.ListAsync(prefix).ConfigureAwait(false);

            if (files == null || files.Count == 0) return;

            await externalFileStorage.RemoveAsync(files.Select(file => file.Path)).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Removes all objects stored under the given prefixes, processing several prefixes at once
    /// to avoid a single network round trip per prefix.
    /// </summary>
    public static async Task RemoveAllUnderPrefixesAsync(this IExternalFileStorage externalFileStorage,
        IEnumerable<string> prefixes)
    {
        using var throttle = new SemaphoreSlim(MaxParallelPrefixDeletions);

        var deletions = prefixes.Select(async prefix =>
        {
            await throttle.WaitAsync().ConfigureAwait(false);

            try
            {
                await externalFileStorage.RemoveAllUnderPrefixAsync(prefix).ConfigureAwait(false);
            }
            finally
            {
                throttle.Release();
            }
        }).ToList();

        await Task.WhenAll(deletions).ConfigureAwait(false);
    }
}
