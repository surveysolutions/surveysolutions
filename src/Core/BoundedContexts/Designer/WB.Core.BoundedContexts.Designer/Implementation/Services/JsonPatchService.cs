using JsonDiffPatchDotNet;
using WB.Core.BoundedContexts.Designer.Services;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
{
    public class JsonPatchService : IPatchGenerator, IPatchApplier
    {
        public string Diff(string left, string right)
        {
            var diff = new JsonDiffPatch();
            var stringDiff = diff.Diff(left, right);
            return stringDiff;
        }

        public string Apply(string document, string patch)
        {
            var diff = new JsonDiffPatch();
            var stringDiff = diff.Patch(document, patch);
            return stringDiff;
        }
    }
}
