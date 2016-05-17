using System.Collections.Generic;
using System.IO;
using System.Linq;
using Machine.Specifications;

namespace WB.Tests.Integration.ResourcesTranslationTests
{
    internal class when_checking_that_translated_resources_have_same_string_format_elements_as_original : ResourcesTranslationTestsContext
    {
        Establish context = () =>
        {
            translatedResourceFiles = Enumerable.Concat(
                TestEnvironment.GetAllFilesFromSourceFolder(string.Empty, "*.??.resx"),
                TestEnvironment.GetAllFilesFromSourceFolder(string.Empty, "*.??-??.resx"));
        };

        Because of = () =>
            translatedResourceStringsNotCorrespondingToOriginal =
                from translatedResourceFile in translatedResourceFiles
                let resourceFileName = Path.GetFileName(translatedResourceFile)
                from inconsistentResource in GetTranslatedResourcesNotCorrespondingToOriginalByStringFormat(translatedResourceFile)
                select $"{resourceFileName}: {inconsistentResource}";

        It should_find_translated_resource_files = () =>
            translatedResourceFiles.ShouldNotBeEmpty();

        It should_find_no_inconsistencies = () =>
            translatedResourceStringsNotCorrespondingToOriginal.ShouldBeEmpty();

        private static IEnumerable<string> translatedResourceFiles;
        private static IEnumerable<string> translatedResourceStringsNotCorrespondingToOriginal;

        private static IEnumerable<string> GetTranslatedResourcesNotCorrespondingToOriginalByStringFormat(string translatedResourceFile)
        {
            string originalResourceFile = ToOriginalResourceFileName(translatedResourceFile);

            Dictionary<string, string> translatedResources = GetStringResourcesFromResX(translatedResourceFile);
            Dictionary<string, string> originalResources = GetStringResourcesFromResX(originalResourceFile);

            foreach (var translatedResource in translatedResources)
            {
                if (!originalResources.ContainsKey(translatedResource.Key))
                {
                    yield return $"{translatedResource.Key}: no original resource string found";
                    continue;
                }

                string originalResourceValue = originalResources[translatedResource.Key];

                string translatedStringFormatEntries = GetStringFormatEntriesAsString(translatedResource.Value);
                string originalStringFormatEntries = GetStringFormatEntriesAsString(originalResourceValue);

                if (translatedStringFormatEntries != originalStringFormatEntries)
                    yield return $"{translatedResource.Key}: has '{translatedStringFormatEntries}', but should have '{originalStringFormatEntries}'";
            }
        }
    }
}