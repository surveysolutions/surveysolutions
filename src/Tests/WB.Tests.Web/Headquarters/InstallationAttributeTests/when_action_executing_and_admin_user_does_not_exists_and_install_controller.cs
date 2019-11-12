using WB.UI.Headquarters.Controllers;
using WB.UI.Headquarters.Filters;


namespace WB.Tests.Unit.Applications.Headquarters.FilterTests.InstallationAttributeTests
{
    internal class when_action_executing_and_admin_user_does_not_exists_and_install_controller : InstallationAttributeTestsContext
    {
        [NUnit.Framework.Test] public void should_exception_be_null () {
            attribute = CreateInstallationAttribute();
            attribute.OnActionExecuting(
                CreateFilterContext(new InstallController(null, null, null, null, null)));
        }

        private static InstallationAttribute attribute;
    }
}
