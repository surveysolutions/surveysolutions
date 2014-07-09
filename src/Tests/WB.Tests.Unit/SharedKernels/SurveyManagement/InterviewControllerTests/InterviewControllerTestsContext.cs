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
            IGlobalInfoProvider globalInfoProvider = null,
            ILogger logger = null,
            IViewFactory<ChangeStatusInputModel, ChangeStatusView> changeStatusFactory = null,
            IViewFactory<InterviewInfoForRevalidationInputModel, InterviewInfoForRevalidationView> revalidateInterviewViewFactory = null,
            IInterviewSummaryViewFactory interviewSummaryViewFactory = null)
        {
            return new InterviewController(
                commandService ?? Mock.Of<ICommandService>(),
                globalInfoProvider ?? Mock.Of<IGlobalInfoProvider>(),
                logger ?? Mock.Of<ILogger>(),
                changeStatusFactory ?? Stub<IViewFactory<ChangeStatusInputModel, ChangeStatusView>>.WithNotEmptyValues,
                revalidateInterviewViewFactory ?? Mock.Of<IViewFactory<InterviewInfoForRevalidationInputModel, InterviewInfoForRevalidationView>>(),
                interviewSummaryViewFactory ?? Stub<IInterviewSummaryViewFactory>.WithNotEmptyValues);
        }
    }
}
