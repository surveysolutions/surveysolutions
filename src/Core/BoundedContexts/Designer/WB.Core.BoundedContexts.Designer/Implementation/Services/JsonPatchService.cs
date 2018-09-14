using JsonDiffPatchDotNet;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.Infrastructure.FileSystem;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
{
    public class JsonPatchService : IPatchGenerator, IPatchApplier
    {
        private readonly IArchiveUtils archiveUtils;
        private const string emptyJson = "{}";

        public JsonPatchService(IArchiveUtils archiveUtils)
        {
            this.archiveUtils = archiveUtils;
        }

        public string Diff(string left, string right)
        {
            var jdp = new JsonDiffPatch();
            var patch = jdp.Diff(left ?? emptyJson, right ?? emptyJson);
            if (string.IsNullOrEmpty(patch))
            {
                return null;
            }

            string stringPatch = archiveUtils.CompressString(patch);
            return stringPatch;
        }

        public string Apply(string document, string patch)
        {
            if (patch == null) return document;

            var jdp = new JsonDiffPatch();

            var patchText = this.archiveUtils.DecompressString(patch);

            var stringDiff = jdp.Patch(string.IsNullOrEmpty(document) ? emptyJson : document, patchText);
            if (stringDiff.Equals(emptyJson))
            {
                return null;
            }

            return stringDiff;
        }
    }
}
