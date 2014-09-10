using Machine.Specifications;
using Moq;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.UI.Headquarters.Controllers;

namespace WB.UI.Headquarters.Tests.InstallControllerTests
{
    [Subject(typeof(InstallController))]
    internal class InstallControllerTestsContext
    {
        protected static InstallController CreateController(ICommandService commandService = null,
            IGlobalInfoProvider globalInfo = null, ILogger logger = null)
        {
            return new InstallController(commandService: commandService ?? Mock.Of<ICommandService>(),
                globalInfo: globalInfo ?? Mock.Of<IGlobalInfoProvider>(), logger: logger ?? Mock.Of<ILogger>());
        }
    }
}
