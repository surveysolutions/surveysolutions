using Machine.Specifications;
using System.Xml.Linq;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.UI.Designer.Api;

namespace WB.Tests.Integration.TesterSettingsTests
{
    internal class when_settings_contain_designer_accesss_point : TesterSettingsTestsContext
    {
        Because of = () =>
        {
            var pathToSettings = TestEnvironment.GetSourcePath("UI/Tester/WB.UI.Tester/Resources/values/settings.xml");

            settings_path = XDocument
                .Load(pathToSettings)
                .Root
                .TreeToEnumerable(_ => _.Elements())
                .Where(element => element.Name == "string")
                .SingleOrDefault(element => element.Attribute("name").Value == "DesignerEndpoint").Value;

            designer_route = GetRoutePrefix(typeof (QuestionnairesController));
        };
        
        It should_correspond_designer_latest_route_path = () =>
            settings_path.Split('/').Last().ShouldEqual(designer_route.Split('/')[1]);

        private static string settings_path;
        private static string designer_route;
    }
}
