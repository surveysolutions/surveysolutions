using Moq;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Tests.Abc;
using WB.UI.Headquarters.Controllers;

namespace WB.Tests.Unit.Applications.Headquarters.ApiUserControllerTests
{
    internal class ApiUserControllerTestContext
    {
        protected static ApiUserController CreateApiUserController(ICommandService commandService = null,
            HqUserManager userManager = null)
        {
            return new ApiUserController(
                commandService ?? Mock.Of<ICommandService>(),
                Mock.Of<ILogger>(),
                Mock.Of<IAuthorizedUser>(),
                userManager ?? Create.Storage.HqUserManager());
        }
    }
}