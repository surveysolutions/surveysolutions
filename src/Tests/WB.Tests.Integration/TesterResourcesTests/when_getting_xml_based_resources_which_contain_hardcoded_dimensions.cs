using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace WB.Tests.Integration.TesterResourcesTests
{
    internal class when_getting_xml_based_resources_which_contain_hardcoded_dimensions : TesterResourcesTestsContext
    {
        [Test]
        public void should_return_only_dimensions_xmls_from_values_folders()
        {
            var resources = GetXmlResourcesHavingHardcodedDimensions("UI/Tester/WB.UI.Tester/Resources").ToArray();

            resources.Should().OnlyContain(resource =>
                resource.ToLower().StartsWith("values") && resource.ToLower().Contains("dimensions"));
        }
    }
}
