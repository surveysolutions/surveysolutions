using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace WB.Tests.Integration.SupervisorResourcesTests
{
    internal class DimensionsTests : SupervisorResourcesTestsContext
    {
        [Test]
        public void should_return_only_dimensions_xmls_from_values_folders()
        {
            var resources = GetXmlResourcesHavingHardcodedDimensions(@"UI\Supervisor\WB.UI.Supervisor\Resources").ToArray();

            resources.Should().OnlyContain(resource =>
                resource.ToLower().StartsWith("values") && resource.ToLower().Contains("dimensions"));
        }


        [Test]
        public void should_return_same_dimensions_from_both_folders()
        {
            var largeDevicesDimensions = GetDimensionsNames("UI/Supervisor/WB.UI.Supervisor/Resources/values-large").ToArray();
            var smallDevicesDimensions = GetDimensionsNames("UI/Supervisor/WB.UI.Supervisor/Resources/values-small").ToArray();

            largeDevicesDimensions.Should().BeEquivalentTo(smallDevicesDimensions);
        }
    }
}
