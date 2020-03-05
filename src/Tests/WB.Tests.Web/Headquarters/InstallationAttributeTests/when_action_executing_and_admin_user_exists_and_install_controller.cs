using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using WB.Tests.Web;
using WB.UI.Headquarters.Filters;

namespace WB.Tests.Unit.Applications.Headquarters.FilterTests.InstallationAttributeTests
{
    internal class when_action_executing_and_admin_user_exists_and_install_controller : InstallationAttributeTestsContext
    {
        [Test]
        public void should_return_404_result()
        {
            var users = Abc.Create.Storage.UserRepository(Create.Entity.HqUser(role: UserRoles.Administrator));
            var attribute = CreateInstallationAttribute();
            var actionExecutingContext = CreateFilterContext(Create.Controller.InstallController(), users);
            
            attribute.OnActionExecuting(actionExecutingContext);

            Assert.That(actionExecutingContext.Result, 
                Is.InstanceOf<NotFoundResult>());
        }
    }
}
