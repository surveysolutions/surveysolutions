using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace WB.Tests.Integration.ResourcesTranslationTests
{
    internal class when_checking_all_translations : ResourcesTranslationTestsContext
    {
        [Test]
        public void should_not_contain_empty_strings()
        {
            var csproj = TestEnvironment.GetAllFilesFromSourceFolder(string.Empty, "*.csproj");
            var translatedResourceFiles = GetAllLinkedResourceFiles(csproj).Where(file => Path.GetFileNameWithoutExtension(file).Contains("."));

            List<string> emptyTranslations = new List<string>();

            foreach (var resourceFile in translatedResourceFiles)
            {
                foreach (KeyValuePair<string, string> translation in GetStringResourcesFromResX(resourceFile))
                {
                    if (string.IsNullOrWhiteSpace(translation.Value))
                    {
                        emptyTranslations.Add($"{Path.GetFileName(resourceFile)}.{translation.Key}");
                    }
                }
            }

            Assert.That(emptyTranslations, Is.Empty);
        }
    }
}