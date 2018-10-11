using WB.UI.Headquarters.Controllers;
using WB.UI.Headquarters.Filters;


namespace WB.Tests.Unit.Applications.Headquarters.FilterTests.InstallationAttributeTests
{
    internal class when_action_executing_and_control_panel_controller : InstallationAttributeTestsContext
    {
        [NUnit.Framework.Test] public void context () {
            var attribute = CreateInstallationAttribute();
            attribute.OnActionExecuting(CreateFilterContext(
                new ControlPanelController(null, 
                null, null, null, null, null, null)));
        }
    }
}
