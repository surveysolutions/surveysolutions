using System;
using FluentAssertions;
using WB.UI.Headquarters.Controllers;
using WB.UI.Headquarters.Filters;


namespace WB.Tests.Unit.Applications.Headquarters.FilterTests.InstallationAttributeTests
{
    internal class when_action_executing_and_control_panel_controller : InstallationAttributeTestsContext
    {
        [NUnit.Framework.Test] public void context () {
            attribute = CreateInstallationAttribute();
            attribute.OnActionExecuting(CreateFilterContext(new ControlPanelController(null, null, null, null, null)));
        }

        private static InstallationAttribute attribute;
        private static Exception exception;
    }
}
