using NUnit.Framework;
using WB.Tests.Web;
using WB.UI.Headquarters.Controllers;
using WB.UI.Headquarters.Filters;


namespace WB.Tests.Unit.Applications.Headquarters.FilterTests.InstallationAttributeTests
{
    internal class when_action_executing_and_admin_user_does_not_exists_and_install_controller : InstallationAttributeTestsContext
    {
        [NUnit.Framework.Test]
        public void should_not_throw_exception()
        {
            attribute = CreateInstallationAttribute();
            
            Assert.DoesNotThrow(() =>
            {
                attribute.OnActionExecuting(CreateFilterContext(Create.Controller.InstallController()));
            });
        }

        private static InstallationFilter attribute;
    }
}
