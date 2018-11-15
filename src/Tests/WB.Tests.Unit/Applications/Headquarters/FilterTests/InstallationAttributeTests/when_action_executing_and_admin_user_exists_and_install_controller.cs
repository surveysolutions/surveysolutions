using System.Web;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Tests.Abc;
using WB.UI.Headquarters.Controllers;
using WB.UI.Headquarters.Filters;

namespace WB.Tests.Unit.Applications.Headquarters.FilterTests.InstallationAttributeTests
{
    internal class when_action_executing_and_admin_user_exists_and_install_controller : InstallationAttributeTestsContext
    {
        [NUnit.Framework.Test]
        public void should_exception_status_code_be_equal_to_404()
        {
            attribute = CreateInstallationAttribute();
            var exception = Assert.Throws<HttpException>(() =>
                attribute.OnActionExecuting(CreateFilterContext(new InstallController(null, null, null, null, null),
                    Create.Storage.UserRepository(Create.Entity.HqUser(role: UserRoles.Administrator)))));

            exception.GetHttpCode().Should().Be(404);
        }

        private static InstallationAttribute attribute;
    }
}
