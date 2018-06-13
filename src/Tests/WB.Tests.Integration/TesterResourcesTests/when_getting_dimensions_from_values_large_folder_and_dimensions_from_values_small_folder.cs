using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace WB.Tests.Integration.TesterResourcesTests
{
    internal class when_getting_dimensions_from_values_large_folder_and_dimensions_from_values_small_folder : TesterResourcesTestsContext
    {
        [OneTimeSetUp]
        public void Because()
        {
            largeDevicesDimensions = GetDimensionsNames("UI/Shared/WB.UI.Shared.Enumerator/Resources/values-large").ToArray();
            smallDevicesDimensions = GetDimensionsNames("UI/Shared/WB.UI.Shared.Enumerator/Resources/values-small").ToArray();
        }

        [Test]
        public void should_return_same_dimensions_from_both_folders() =>
            largeDevicesDimensions.Should().BeEquivalentTo(smallDevicesDimensions);

        private static string[] largeDevicesDimensions;
        private static string[] smallDevicesDimensions;
    }
}
