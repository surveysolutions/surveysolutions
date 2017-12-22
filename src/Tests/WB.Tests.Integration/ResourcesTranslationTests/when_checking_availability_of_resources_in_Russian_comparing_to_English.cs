using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace WB.Tests.Integration.ResourcesTranslationTests
{
    internal class when_checking_availability_of_resources_in_Russian_comparing_to_English : ResourcesTranslationTestsContext
    {
        [OneTimeSetUp]
        public void Because()
        {
           russianResourceFiles = TestEnvironment
                .GetAllFilesFromSourceFolder(string.Empty, "*.ru.resx")
                .Where(IsEnumeratorOrTesterOrInterviwerFile);

            englishResourceFiles = TestEnvironment
                .GetAllFilesFromSourceFolder(string.Empty, "*.resx")
                .Except(TestEnvironment.GetAllFilesFromSourceFolder(string.Empty, "*.??.resx"))
                .Except(TestEnvironment.GetAllFilesFromSourceFolder(string.Empty, "*.??-??.resx"))
                .Where(IsEnumeratorOrTesterOrInterviwerFile);

            russianResourceNames =
                from resourceFile in russianResourceFiles
                let resourceFileName = GetTranslatedResourceFileNameWithoutExtension(resourceFile)
                from resourceName in GetStringResourceNamesFromResX(resourceFile)
                select $"{resourceFileName}: {resourceName}";

            englishResourceNames =
                from resourceFile in englishResourceFiles
                let resourceFileName = GetOriginalResourceFileNameWithoutExtension(resourceFile)
                from resourceName in GetStringResourceNamesFromResX(resourceFile)
                select $"{resourceFileName}: {resourceName}";
        }
 
        [Test]
        public void should_find_Russian_resource_files() => russianResourceFiles.Should().NotBeEmpty();

        [Test]
        public void should_find_English_resource_files () => englishResourceFiles.Should().NotBeEmpty();

        [Test]
        public void should_be_the_same_set_of_resources_in_Russian_as_it_is_in_English() =>
            Assert.That(russianResourceNames, Is.EqualTo(englishResourceNames));

        private IEnumerable<string> russianResourceFiles;
        private IEnumerable<string> englishResourceFiles;
        private IEnumerable<string> russianResourceNames;
        private IEnumerable<string> englishResourceNames;

        private static bool IsEnumeratorOrTesterOrInterviwerFile(string filePath)
        {
            return filePath.Contains("Enumerator") || filePath.Contains("Tester") || filePath.Contains("Interviewer");
        }
    }
}