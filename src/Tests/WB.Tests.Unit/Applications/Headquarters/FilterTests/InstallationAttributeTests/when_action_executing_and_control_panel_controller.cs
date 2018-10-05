using AutoFixture;
using WB.Tests.Abc;
using WB.UI.Headquarters.Controllers;

namespace WB.Tests.Unit.Applications.Headquarters.FilterTests.InstallationAttributeTests
{
    internal class when_action_executing_and_control_panel_controller : InstallationAttributeTestsContext
    {
        [NUnit.Framework.Test] public void context () {
            var attribute = CreateInstallationAttribute();

            attribute.OnActionExecuting(CreateFilterContext(
                Create.Other.AutoFixture().Create<ControlPanelController>()));
        }
    }
}
