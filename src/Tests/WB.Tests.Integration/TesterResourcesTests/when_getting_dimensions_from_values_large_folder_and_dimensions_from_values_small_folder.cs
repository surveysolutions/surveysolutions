using System.Linq;
using Machine.Specifications;

namespace WB.Tests.Integration.TesterResourcesTests
{
    internal class when_getting_dimensions_from_values_large_folder_and_dimensions_from_values_small_folder : TesterResourcesTestsContext
    {
        Because of = () =>
        {
            largeDevicesDimensions = GetDimensionsNames("UI/QuestionnaireTester/WB.UI.Tester/Resources/values-large").ToArray();
            smallDevicesDimensions = GetDimensionsNames("UI/QuestionnaireTester/WB.UI.Tester/Resources/values-small").ToArray();
        };

        It should_return_same_dimensions_from_both_folders = () =>
            largeDevicesDimensions.ShouldContainOnly(smallDevicesDimensions);

        private static string[] largeDevicesDimensions;
        private static string[] smallDevicesDimensions;
    }
}