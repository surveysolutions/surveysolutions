using Machine.Specifications;
using Main.Core.View;
using Moq;
using Ncqrs.Commanding.ServiceModel;
using Questionnaire.Core.Web.Helpers;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernels.SurveyManagement.Views.ChangeStatus;
using WB.Core.SharedKernels.SurveyManagement.Views.Revalidate;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;

namespace WB.UI.Supervisor.Tests.InterviewControllerTests
{
    [Subject(typeof(InterviewController))]
    internal class InterviewControllerTestsContext
    {
        protected static InterviewController CreateController(ICommandService commandService = null,
            IGlobalInfoProvider provider = null, ILogger logger = null,
            IViewFactory<ChangeStatusInputModel, ChangeStatusView> changeStatusFactory = null,
            IViewFactory<InterviewInfoForRevalidationInputModel, InterviewInfoForRevalidationView> revalidateInterviewViewFactory = null)
        {
            return new InterviewController(commandService: commandService ?? Mock.Of<ICommandService>(),
                provider: provider ?? Mock.Of<IGlobalInfoProvider>(), logger: logger ?? Mock.Of<ILogger>(),
                changeStatusFactory:
                    changeStatusFactory ?? Mock.Of<IViewFactory<ChangeStatusInputModel, ChangeStatusView>>(),
                revalidateInterviewViewFactory:
                    revalidateInterviewViewFactory ??
                    Mock.Of<IViewFactory<InterviewInfoForRevalidationInputModel, InterviewInfoForRevalidationView>>());
        }
    }
}
