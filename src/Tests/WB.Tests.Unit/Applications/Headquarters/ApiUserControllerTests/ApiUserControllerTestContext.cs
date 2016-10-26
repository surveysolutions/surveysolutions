using Moq;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.UI.Headquarters.Controllers;

namespace WB.Tests.Unit.Applications.Headquarters.ApiUserControllerTests
{
    internal class ApiUserControllerTestContext
    {
        protected static ApiUserController CreateApiUserController(ICommandService commandService = null,
            IIdentityManager identityManager = null)
        {
            return new ApiUserController(
                commandService ?? Mock.Of<ICommandService>(),
                Mock.Of<ILogger>(),
                identityManager ?? Mock.Of<IIdentityManager>());
        }
    }
}