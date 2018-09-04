using System;
using JsonDiffPatchDotNet;
using Newtonsoft.Json;
using WB.Core.BoundedContexts.Designer.Services;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
{
    public class JsonPatchService : IPatchGenerator, IPatchApplier
    {
        private const string emptyJson = "{}";

        public string Diff(string left, string right)
        {
            var diff = new JsonDiffPatch();
            var stringDiff = diff.Diff(string.IsNullOrEmpty(left) ? emptyJson : left, 
                                       string.IsNullOrEmpty(right) ? emptyJson : right);

            if (stringDiff == null) return null;

            var removedFormatting = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(stringDiff), Formatting.None);
            return removedFormatting;
        }

        public string Apply(string document, string patch)
        {
            if (patch == null) return document;

            var diff = new JsonDiffPatch();
            var stringDiff = diff.Patch(string.IsNullOrEmpty(document) ? emptyJson : document, patch);
            return stringDiff;
        }
    }
}
