using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Machine.Specifications;

namespace WB.Tests.Integration.TesterResourcesTests
{
    internal class when_getting_xml_based_resources_which_contain_hardcoded_dimensions : TesterResourcesTestsContext
    {
        Because of = () =>
            resources = GetXmlResourcesHavingHardcodedDimensions("UI/QuestionnaireTester/WB.UI.Tester/Resources").ToArray();

        It should_return_only_dimensions_xmls_from_values_folders = () =>
            resources.ShouldEachConformTo(resource => resource.ToLower().StartsWith("values") && resource.ToLower().Contains("dimensions"));

        private static string[] resources;
    }
}
