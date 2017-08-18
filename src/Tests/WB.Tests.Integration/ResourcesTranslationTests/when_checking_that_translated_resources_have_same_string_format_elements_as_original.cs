using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Machine.Specifications;
using NUnit.Framework;

namespace WB.Tests.Integration.ResourcesTranslationTests
{
    [NUnit.Framework.Ignore("temp")]
    internal class when_checking_that_translated_resources_have_same_string_format_elements_as_original : ResourcesTranslationTestsContext
    {
        [OneTimeSetUp]
        public void Context()
        {
            var csproj = TestEnvironment.GetAllFilesFromSourceFolder(string.Empty, "*.csproj");
            translatedResourceFiles = GetAllLinkedResourceFiles(csproj).Where(file => Path.GetFileNameWithoutExtension(file).Contains("."));

            this.Because();
        }

        private void Because() => translatedResourceStringsNotCorrespondingToOriginal =
            from translatedResourceFile in translatedResourceFiles
            let resourceFileName = Path.GetFileName(translatedResourceFile)
            from inconsistentResource in GetTranslatedResourcesNotCorrespondingToOriginalByStringFormat(translatedResourceFile)
            select $"{resourceFileName}: {inconsistentResource}";

        [Test]
        public void should_find_translated_resource_files() => translatedResourceFiles.ShouldNotBeEmpty();
        
        [Test]
        public void should_find_no_inconsistencies () => translatedResourceStringsNotCorrespondingToOriginal.ShouldBeEmpty();

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
                    yield return $"<{translatedResourceFile}> {translatedResource.Key}: no original resource string found";
                    continue;
                }

                string originalResourceValue = originalResources[translatedResource.Key];

                string translatedStringFormatEntries = GetStringFormatEntriesAsString(translatedResource.Value);
                string originalStringFormatEntries = GetStringFormatEntriesAsString(originalResourceValue);

                if (translatedStringFormatEntries != originalStringFormatEntries)
                    yield return $"<{translatedResourceFile}> {translatedResource.Key}: has '{translatedStringFormatEntries}', but should have '{originalStringFormatEntries}'";
            }
        }

        public  IEnumerable<string> GetAllLinkedResourceFiles(IEnumerable<string> csprojFiles)
        {
            foreach (var csproj in csprojFiles)
            {
                using (XmlReader reader = XmlReader.Create(csproj))
                {
                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            if (string.Equals(reader.Name, "Content", StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(reader.Name, "EmbeddedResource", StringComparison.OrdinalIgnoreCase))
                            {
                                while (reader.MoveToNextAttribute())
                                {
                                    if (string.Equals(reader.Name, "Include", StringComparison.OrdinalIgnoreCase))
                                    {
                                        if (reader.Value.EndsWith(".resx", StringComparison.OrdinalIgnoreCase))
                                        {
                                            var fi = new FileInfo(csproj);
                                            yield return Path.Combine(fi.DirectoryName, reader.Value);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}