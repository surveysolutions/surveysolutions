using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace WB.Tests.Integration.ResourcesTranslationTests
{
    [TestFixture]
    internal class ResourcesTranslationTests : ResourcesTranslationTestsContext
    {
        [Test]
        public void when_checking_that_translated_resources_have_same_property_names_in_format_elements_as_original()
        {
            var csproj = TestEnvironment.GetAllFilesFromSourceFolder(string.Empty, "*.csproj");
            var translatedResourceFiles = GetAllLinkedResourceFiles(csproj).Where(file => Path.GetFileNameWithoutExtension(file).Contains(".")).ToList();

            var translatedResourceStringsNotCorrespondingToOriginal =
                from translatedResourceFile in translatedResourceFiles
                let resourceFileName = Path.GetFileName(translatedResourceFile)
                from inconsistentResource in GetTranslatedResourcesNotCorrespondingToOriginalByStringFormat(translatedResourceFile)
                select $"{resourceFileName}: {inconsistentResource}";

            Assert.IsNotEmpty(translatedResourceFiles);
            Assert.IsEmpty(translatedResourceStringsNotCorrespondingToOriginal);
        }

        private IEnumerable<string> GetTranslatedResourcesNotCorrespondingToOriginalByStringFormat(string translatedResourceFile)
        {
            string originalResourceFile = ToOriginalResourceFileName(translatedResourceFile);

            Dictionary<string, string> translatedResources = GetStringResourcesFromResX(translatedResourceFile);
            Dictionary<string, string> originalResources = GetStringResourcesFromResX(originalResourceFile);

            foreach (var translatedResource in translatedResources)
            {
                if (!originalResources.ContainsKey(translatedResource.Key))
                {
                    yield return $"<{translatedResourceFile}> {translatedResource.Key}: no original resource string found";
                    continue;
                }

                string originalResourceValue = originalResources[translatedResource.Key];

                string translatedStringFormatEntries = GetUiStringFormatEntriesAsString(translatedResource.Value);
                string originalStringFormatEntries = GetUiStringFormatEntriesAsString(originalResourceValue);

                if (translatedStringFormatEntries != originalStringFormatEntries)
                    yield return $"<{translatedResourceFile}> {translatedResource.Key}: has '{translatedStringFormatEntries}', but should have '{originalStringFormatEntries}'";
            }
        }
    }
}