using Moq;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.UI.Headquarters.Controllers;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Tests.Unit.Applications.Headquarters.ApiUserControllerTests
{
    internal class ApiUserControllerTestContext
    {
        protected static ApiUserController CreateApiUserController(ICommandService commandService = null, 
            IGlobalInfoProvider globalInfoProvider = null, IUserViewFactory userViewFactory = null)
        {
            return new ApiUserController(
                commandService ?? Mock.Of<ICommandService>(),
                globalInfoProvider ?? Mock.Of<IGlobalInfoProvider>(),
                Mock.Of<ILogger>(),
                userViewFactory ?? Mock.Of<IUserViewFactory>(),
                Mock.Of<IPasswordHasher>());
        }
    }
}