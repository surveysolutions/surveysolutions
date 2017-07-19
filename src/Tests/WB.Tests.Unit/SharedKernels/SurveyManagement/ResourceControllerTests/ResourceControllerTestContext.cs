using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.Implementation;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.UI.Headquarters.Controllers;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ResourceControllerTests
{
    [TestOf(typeof(ResourceController))]
    internal class ResourceControllerTestContext
    {
        protected static ResourceController CreateController(ICommandService commandService = null,
            IAuthorizedUser globalInfoProvider = null,
            ILogger logger = null, IImageFileStorage imageFileStorage = null)
        {
            return new ResourceController(
                commandService ?? Mock.Of<ICommandService>(),
                logger ?? Mock.Of<ILogger>(), 
                imageFileStorage ?? Mock.Of<IImageFileStorage>(),
                new InMemoryPlainStorageAccessor<AudioFile>());
        }
    }
}
