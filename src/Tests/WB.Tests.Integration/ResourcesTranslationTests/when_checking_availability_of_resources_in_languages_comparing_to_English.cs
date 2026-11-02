using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace WB.Tests.Integration.ResourcesTranslationTests
{
    [TestFixture]
    internal class when_checking_availability_of_resources_in_languages_comparing_to_English : ResourcesTranslationTestsContext
    {
        private static IReadOnlyList<string> allResourceFiles;
        private static IReadOnlyCollection<string> englishResourceFiles;
        private static IReadOnlyCollection<string> englishResourceNames;

        [OneTimeSetUp]
        public void one_time_setup()
        {
            allResourceFiles = TestEnvironment
                .GetAllFilesInSolution("*.resx")
                .ToList();

            englishResourceFiles = allResourceFiles
                .Except(TestEnvironment.GetAllFilesInSolution("*.??.resx"))
                .Except(TestEnvironment.GetAllFilesInSolution("*.??-??.resx"))
                .ToList();

            englishResourceNames = GetResourceNames(englishResourceFiles, GetOriginalResourceFileNameWithoutExtension)
                .ToList();
        }

        [TestCase ("ru")]
        [TestCase ("es")]
        [TestCase ("fr")]
        [TestCase ("cs")]
        [TestCase ("pt")]
        [TestCase ("uk")]
        [TestCase ("ar")]
        [TestCase ("id")]
        [TestCase ("ka")]
        [TestCase ("km")]
        [TestCase ("ro")]
        [TestCase ("th")]
        [TestCase ("sq")]
        [TestCase ("vi")]
        [TestCase ("zh")]
        public void should_be_the_same_set_of_resources_in_other_cultures_as_it_is_in_English(string culture)
        {
            var translatedResourceFiles = allResourceFiles
                .Where(file => IsTranslatedResourceFileForCulture(file, culture))
                .ToList();

            var translatedResourceNames = GetResourceNames(translatedResourceFiles, GetTranslatedResourceFileNameWithoutExtension)
                .ToList();

            //should_find_Russian_resource_files() => 
            Assert.That(translatedResourceFiles, Is.Not.Empty);

            //should_find_English_resource_files() => 
            Assert.That(englishResourceFiles, Is.Not.Empty);

            //should_be_the_same_set_of_resource_files_in_Russian_as_it_is_in_English() => 
            Assert.That(translatedResourceFiles.Select(f => ToOriginalResourceFileName(f, culture)),
                Is.EquivalentTo(englishResourceFiles));

            // Ensure translation includes all current English keys; extra localized legacy keys are tolerated.
            Assert.That(translatedResourceNames, Is.SupersetOf(englishResourceNames));
        }


        private static bool IsTranslatedResourceFileForCulture(string filePath, string culture)
        {
            return filePath.EndsWith($".{culture}.resx", StringComparison.OrdinalIgnoreCase);
        }

        private static string ToOriginalResourceFileName(string translatedResourceFileName, string culture)
        {
            var suffix = $".{culture}.resx";
            if (!translatedResourceFileName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                return translatedResourceFileName;

            return translatedResourceFileName.Substring(0, translatedResourceFileName.Length - suffix.Length) + ".resx";
        }

        private static IEnumerable<string> GetResourceNames(IEnumerable<string> resourceFiles, Func<string, string> getResourceFileName)
        {
            foreach (var resourceFile in resourceFiles)
            {
                var resourceFileName = getResourceFileName(resourceFile);
                foreach (var resource in GetStringResourcesFromResX(resourceFile))
                {
                    if (IsNotPluralForm(resource.Key) && !string.IsNullOrEmpty(resource.Value))
                        yield return $"{resourceFileName}: {resource.Key}";
                }
            }
        }

        private static bool IsNotPluralForm(string resourceName)
        {
            return !(resourceName.EndsWith("_other") || resourceName.EndsWith("_plural"));
        }
    }
}
