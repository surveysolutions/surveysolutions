using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace WB.Tests.Integration.ResourcesTranslationTests
{
    [TestFixture]
    internal class when_checking_availability_of_resources_in_languages_comparing_to_English : ResourcesTranslationTestsContext
    {
        [TestCase ("ru")]
        [TestCase ("es")]
        [TestCase ("fr")]
        [TestCase ("cs")]
        [TestCase ("pt")]
        //[TestCase ("uk")]
        [TestCase ("ar")]
        //[TestCase ("id")]
        //[TestCase ("ka")]
        //[TestCase ("km")]
        
        //[TestCase ("ro")]
        //[TestCase ("th")]
        //[TestCase ("sq")]
        //[TestCase ("vi")]
        //[TestCase ("zh")]
        public void should_be_the_same_set_of_resources_in_other_cultures_as_it_is_in_English(string culture)
        {
            russianResourceFiles = TestEnvironment
                .GetAllFilesInSolution($"*.{culture}.resx")
                .ToList();

            englishResourceFiles = TestEnvironment
                .GetAllFilesInSolution("*.resx")
                .Except(TestEnvironment.GetAllFilesInSolution("*.??.resx"))
                .Except(TestEnvironment.GetAllFilesInSolution("*.??-??.resx"))
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
            Assert.That(russianResourceFiles.Select(f => f.Replace($".{culture}.", ".")).OrderBy(x => x), 
                Is.EquivalentTo(englishResourceFiles.OrderBy(x => x)));

            // should_be_the_same_set_of_resources_in_Russian_as_it_is_in_English() =>
            Assert.That(russianResourceNames.OrderBy(x => x), Is.EquivalentTo(englishResourceNames.OrderBy(x => x)));
        }


        private IEnumerable<string> russianResourceFiles;
        private IEnumerable<string> englishResourceFiles;
        private IEnumerable<string> russianResourceNames;
        private IEnumerable<string> englishResourceNames;

        private static bool IsNotPluralForm(string resourceName)
        {
            return !(resourceName.EndsWith("_other") || resourceName.EndsWith("_plural"));
        }
    }
}
