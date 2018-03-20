using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace WB.Tests.Integration.TesterResourcesTests
{
    internal class when_getting_xml_based_resources_which_contain_hardcoded_dimensions : TesterResourcesTestsContext
    {
        [OneTimeSetUp]
        public void Because() =>
            resources = GetXmlResourcesHavingHardcodedDimensions("UI/Tester/WB.UI.Tester/Resources").ToArray();

        [Test]
        public void should_return_only_dimensions_xmls_from_values_folders() =>
            resources.Should().OnlyContain(resource => resource.ToLower().StartsWith("values") && resource.ToLower().Contains("dimensions"));

        private static string[] resources;
    }
}
