using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.UI.Headquarters.Controllers;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ResourceControllerTests
{
    [Subject(typeof(ResourceController))]
    internal class ResourceControllerTestContext
    {
        protected static ResourceController CreateController(ICommandService commandService = null,
            IAuthorizedUser globalInfoProvider = null,
            ILogger logger = null, IImageFileStorage imageFileStorage = null)
        {
            return new ResourceController(
                commandService ?? Mock.Of<ICommandService>(),
                logger ?? Mock.Of<ILogger>(), imageFileStorage ?? Mock.Of<IImageFileStorage>());
        }
    }
}
