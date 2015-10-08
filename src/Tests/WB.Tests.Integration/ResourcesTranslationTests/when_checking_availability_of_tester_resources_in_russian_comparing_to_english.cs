using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;

namespace WB.Tests.Integration.ResourcesTranslationTests
{
    [Ignore("KP-6181")]
    internal class when_checking_availability_of_tester_resources_in_russian_comparing_to_english : ResourcesTranslationTestsContext
    {
        Because of = () =>
        {
            englishResourceNames = GetStringResourceNamesFromResX(@"Core\BoundedContexts\Tester\WB.Core.BoundedContexts.Tester\Properties\TesterUIResources.resx").ToList();
            russianResourceNames = GetStringResourceNamesFromResX(@"Core\BoundedContexts\Tester\WB.Core.BoundedContexts.Tester\Properties\TesterUIResources.ru-RU.resx").ToList();
        };

        It should_be_the_same_set_of_resources_in_russian_as_it_is_in_english = () =>
            russianResourceNames.ShouldContainOnly(englishResourceNames);

        private static List<string> englishResourceNames;
        private static List<string> russianResourceNames;
    }
}