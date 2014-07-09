using Machine.Specifications;
using Main.Core.View;
using Moq;
using Ncqrs.Commanding.ServiceModel;
using Questionnaire.Core.Web.Helpers;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernels.SurveyManagement.Views.ChangeStatus;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Revalidate;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.InterviewControllerTests
{
    internal class InterviewControllerTestsContext
    {
        protected static InterviewController CreateController(
            ICommandService commandService = null,
            IGlobalInfoProvider provider = null,
            ILogger logger = null,
            IViewFactory<ChangeStatusInputModel, ChangeStatusView> changeStatusFactory = null,
            IViewFactory<InterviewInfoForRevalidationInputModel, InterviewInfoForRevalidationView> revalidateInterviewViewFactory = null)
        {
            return new InterviewController(
                commandService ?? Mock.Of<ICommandService>(),
                provider ?? Mock.Of<IGlobalInfoProvider>(),
                logger ?? Mock.Of<ILogger>(),
                changeStatusFactory ?? Mock.Of<IViewFactory<ChangeStatusInputModel, ChangeStatusView>>(),
                revalidateInterviewViewFactory ?? Mock.Of<IViewFactory<InterviewInfoForRevalidationInputModel, InterviewInfoForRevalidationView>>(),
                Mock.Of<IInterviewSummaryViewFactory>());
        }
    }
}
