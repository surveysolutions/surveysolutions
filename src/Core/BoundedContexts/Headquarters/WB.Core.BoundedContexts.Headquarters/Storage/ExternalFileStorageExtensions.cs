using System;
using System.Linq;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.Headquarters.Storage;

public static class ExternalFileStorageExtensions
{
    // single list/delete request is limited to 1000 objects, so removal is done in a loop
    // until there is nothing left under the prefix
    private const int MaxDeletionBatches = 1000;

    /// <summary>
    /// There are no directories in S3, so all objects stored under the prefix
    /// have to be listed and removed by their own keys.
    /// </summary>
    public static async Task RemoveAllUnderPrefixAsync(this IExternalFileStorage externalFileStorage, string prefix)
    {
        for (var batch = 0; batch < MaxDeletionBatches; batch++)
        {
            var files = await externalFileStorage.ListAsync(prefix).ConfigureAwait(false);

            if (files == null || files.Count == 0) return;

            await externalFileStorage.RemoveAsync(files.Select(file => file.Path)).ConfigureAwait(false);
        }

        var leftovers = await externalFileStorage.ListAsync(prefix).ConfigureAwait(false);

        if (leftovers != null && leftovers.Count > 0)
            throw new InvalidOperationException($"Unable to remove all files stored under '{prefix}'.");
    }
}
