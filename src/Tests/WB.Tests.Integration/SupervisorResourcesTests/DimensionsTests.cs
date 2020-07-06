using System;
using System.IO;
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
            var resourcesRelativePath = Path.Combine("UI", "Supervisor", "WB.UI.Supervisor", "Resources");
            var resources = GetXmlResourcesHavingHardcodedDimensions(resourcesRelativePath).ToArray();

            resources.Should().OnlyContain(resource =>
                resource.StartsWith("values", StringComparison.OrdinalIgnoreCase) && 
                resource.Contains("dimensions", StringComparison.CurrentCultureIgnoreCase));
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
