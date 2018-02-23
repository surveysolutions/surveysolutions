using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace WB.Tests.Integration.ResourcesTranslationTests
{
    [TestFixture]
    internal class when_checking_availability_of_resources_in_Russian_comparing_to_English : ResourcesTranslationTestsContext
    {
        [Test]
        public void should_be_the_same_set_of_resources_in_Russian_as_it_is_in_English()
        {
            russianResourceFiles = TestEnvironment
                .GetAllFilesFromSourceFolder(string.Empty, "*.ru.resx")
                .Where(IsShouldBeTranslatedFile)
                .ToList();

            englishResourceFiles = TestEnvironment
                .GetAllFilesFromSourceFolder(string.Empty, "*.resx")
                .Except(TestEnvironment.GetAllFilesFromSourceFolder(string.Empty, "*.??.resx"))
                .Except(TestEnvironment.GetAllFilesFromSourceFolder(string.Empty, "*.??-??.resx"))
                .Where(IsShouldBeTranslatedFile)
                .ToList();

            russianResourceNames =
                from resourceFile in russianResourceFiles
                let resourceFileName = GetTranslatedResourceFileNameWithoutExtension(resourceFile)
                from resource in GetStringResourcesFromResX(resourceFile)
                where IsNotPluralForm(resource.Key) && !string.IsNullOrEmpty(resource.Value)
                select $"{resourceFileName}: {resource.Key}";

            englishResourceNames =
                from resourceFile in englishResourceFiles
                let resourceFileName = GetOriginalResourceFileNameWithoutExtension(resourceFile)
                from resource in GetStringResourcesFromResX(resourceFile)
                where IsNotPluralForm(resource.Key) && !string.IsNullOrEmpty(resource.Value)
                select $"{resourceFileName}: {resource.Key}";


            //should_find_Russian_resource_files() => 
            Assert.That(russianResourceFiles, Is.Not.Empty);

            //should_find_English_resource_files() => 
            Assert.That(englishResourceFiles, Is.Not.Empty);

            //should_be_the_same_set_of_resource_files_in_Russian_as_it_is_in_English() => 
            Assert.That(russianResourceFiles.Select(f => f.Replace(".ru.", ".")), Is.EqualTo(englishResourceFiles));

            // should_be_the_same_set_of_resources_in_Russian_as_it_is_in_English() =>
            Assert.That(russianResourceNames, Is.EqualTo(englishResourceNames));
        }


        private IEnumerable<string> russianResourceFiles;
        private IEnumerable<string> englishResourceFiles;
        private IEnumerable<string> russianResourceNames;
        private IEnumerable<string> englishResourceNames;

        private static bool IsNotPluralForm(string resourceName)
        {
            return !resourceName.EndsWith("_plural");
        }

        private static bool IsShouldBeTranslatedFile(string filePath)
        {
            var isAdroidApp = filePath.Contains("Enumerator") || filePath.Contains("Tester") || filePath.Contains("Interviewer");
            if (isAdroidApp)
                return true;

            var ignoreResxFiles = new[]
            {
                @"WB.Core.BoundedContexts.Headquarters\Resources\PreloadingVerificationMessages",
                @"Resources\BatchUpload",
                @"WB.UI.Headquarters\Resources\SyncLogMessages",
            };

            if (ignoreResxFiles.Any(filePath.Contains))
                return false;

            return true;
        }
    }
}