using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;

namespace WB.Tests.Integration.ResourcesTranslationTests
{
    internal class when_checking_availability_of_resources_in_Russian_comparing_to_English : ResourcesTranslationTestsContext
    {
        Because of = () =>
        {
            russianResourceFiles = TestEnvironment
                .GetAllFilesFromSourceFolder(string.Empty, "*.ru-RU.resx")
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
                select string.Format("{0}: {1}", resourceFileName, resourceName);

            englishResourceNames =
                from resourceFile in englishResourceFiles
                let resourceFileName = GetOriginalResourceFileNameWithoutExtension(resourceFile)
                from resourceName in GetStringResourceNamesFromResX(resourceFile)
                select string.Format("{0}: {1}", resourceFileName, resourceName);
        };

        It should_find_Russian_resource_files = () =>
            russianResourceFiles.ShouldNotBeEmpty();

        It should_find_English_resource_files = () =>
            englishResourceFiles.ShouldNotBeEmpty();

        It should_be_the_same_set_of_resources_in_Russian_as_it_is_in_English = () =>
            russianResourceNames.ShouldContainOnly(englishResourceNames);

        private static IEnumerable<string> russianResourceFiles;
        private static IEnumerable<string> englishResourceFiles;
        private static IEnumerable<string> russianResourceNames;
        private static IEnumerable<string> englishResourceNames;

        private static bool IsEnumeratorOrTesterOrInterviwerFile(string filePath)
        {
            return filePath.Contains("Enumerator") || filePath.Contains("Tester") || filePath.Contains("Interviewer");
        }
    }
}