using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using FluentAssertions;
using NUnit.Framework;

namespace WB.Tests.Integration.ResourcesTranslationTests
{
    internal class when_checking_that_translated_resources_have_same_string_format_elements_as_original : ResourcesTranslationTestsContext
    {
        private static readonly Regex PluralizationRegex = new Regex(@"(_plural|_\d+)$", RegexOptions.Compiled);

        private static readonly List<string> fileNamesToExculde = new List<string>();

        [OneTimeSetUp]
        public void Context()
        {
            var csproj = TestEnvironment.GetAllFilesFromSourceFolder(string.Empty, "*.csproj");
            translatedResourceFiles = GetAllLinkedResourceFiles(csproj).Where(file => Path.GetFileNameWithoutExtension(file).Contains("."))
                .Where(file => !fileNamesToExculde.Any(x => Path.GetFileName(file).Contains(x)));

            this.Because();
        }

        private void Because() => translatedResourceStringsNotCorrespondingToOriginal =
            from translatedResourceFile in translatedResourceFiles
            let resourceFileName = Path.GetFileName(translatedResourceFile)
            from inconsistentResource in GetTranslatedResourcesNotCorrespondingToOriginalByStringFormat(translatedResourceFile)
            select $"{resourceFileName}: {inconsistentResource}";

        [Test]
        public void should_find_translated_resource_files() => translatedResourceFiles.Should().NotBeEmpty();
        
        [Test]
        public void should_find_no_inconsistencies ()
        {
            var translations = translatedResourceStringsNotCorrespondingToOriginal.ToList();
            
            Assert.That(translations, Has.Count.EqualTo(0), string.Join("\r\n", translations));
        }

        private IEnumerable<string> translatedResourceFiles;
        private IEnumerable<string> translatedResourceStringsNotCorrespondingToOriginal;

        private IEnumerable<string> GetTranslatedResourcesNotCorrespondingToOriginalByStringFormat(string translatedResourceFile)
        {
            string originalResourceFile = ToOriginalResourceFileName(translatedResourceFile);

            Dictionary<string, string> translatedResources = GetStringResourcesFromResX(translatedResourceFile);
            Dictionary<string, string> originalResources = GetStringResourcesFromResX(originalResourceFile);

            foreach (var translatedResource in translatedResources)
            {
                if (!originalResources.ContainsKey(translatedResource.Key))
                {
                    var cleanKey = PluralizationRegex.Replace(translatedResource.Key, "");
                    if (!originalResources.ContainsKey(cleanKey))
                    {
                        yield return $"<{translatedResourceFile}> {translatedResource.Key}: no original resource string found";
                    }

                    continue;
                }

                string originalResourceValue = originalResources[translatedResource.Key];

                string translatedStringFormatEntries = GetStringFormatEntriesAsString(translatedResource.Value);
                string originalStringFormatEntries = GetStringFormatEntriesAsString(originalResourceValue);

                if (translatedStringFormatEntries != originalStringFormatEntries)
                    yield return $"{translatedResource.Key} <{translatedResourceFile}>: has '{translatedStringFormatEntries}', but should have '{originalStringFormatEntries}'";
            }
        }
    }
}
