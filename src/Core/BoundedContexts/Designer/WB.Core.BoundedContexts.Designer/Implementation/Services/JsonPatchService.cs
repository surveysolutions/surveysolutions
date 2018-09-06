using System;
using System.Collections.Generic;
using DiffMatchPatch;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
{
    public class JsonPatchService : IPatchGenerator, IPatchApplier
    {
        private readonly IArchiveUtils archiveUtils;
        private const string emptyJson = "";

        public JsonPatchService(IArchiveUtils archiveUtils)
        {
            this.archiveUtils = archiveUtils;
        }

        public string Diff(string left, string right)
        {
            diff_match_patch patcher = new diff_match_patch();
            List<Patch> patches = patcher.patch_make(left ?? emptyJson,
                                                     right ?? emptyJson);
            if (patches.Count == 0) return null;

            string result = patcher.patch_toText(patches);

            string stringPatch = archiveUtils.CompressString(result);
            return stringPatch;
        }

        public string Apply(string document, string patch)
        {
            if (patch == null) return document;

            diff_match_patch patcher = new diff_match_patch();

            List<Patch> patches = patcher.patch_fromText(this.archiveUtils.DecompressString(patch));

            var stringDiff = patcher.patch_apply(patches, string.IsNullOrEmpty(document) ? emptyJson : document);
            foreach (bool applyResult in (bool[])stringDiff[1])
            {
                if (applyResult == false)
                {
                    throw new ApplicationException("Failed to apply patch");
                }
            }

            var apply = (string)stringDiff[0];
            return apply.IsNullOrEmpty() ? null : apply;
        }
    }
}
