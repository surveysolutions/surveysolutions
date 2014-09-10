using Machine.Specifications;
using Main.Core.View;
using Moq;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernels.SurveyManagement.Views.ChangeStatus;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Interviews;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.InterviewApiControllerTests
{
    [Subject(typeof(InterviewApiController))]
    internal class InterviewApiControllerTestsContext
    {
        protected static InterviewApiController CreateController(ICommandService commandService = null,
            IGlobalInfoProvider globalInfoProvider = null, ILogger logger = null,
            IViewFactory<AllInterviewsInputModel, AllInterviewsView> allInterviewsViewFactory = null,
            IViewFactory<TeamInterviewsInputModel, TeamInterviewsView> teamInterviewViewFactory = null,
            IViewFactory<ChangeStatusInputModel, ChangeStatusView> changeStatusFactory = null,
            IViewFactory<InterviewDetailsInputModel, InterviewDetailsView> interviewDetailsFactory = null,
            IInterviewSummaryViewFactory interviewSummaryViewFactory = null)
        {
            return new InterviewApiController(
                commandService ?? Mock.Of<ICommandService>(),
                globalInfoProvider ?? Mock.Of<IGlobalInfoProvider>(), 
                logger ?? Mock.Of<ILogger>(),
                allInterviewsViewFactory ?? Stub<IViewFactory<AllInterviewsInputModel, AllInterviewsView>>.WithNotEmptyValues,
                teamInterviewViewFactory ?? Stub<IViewFactory<TeamInterviewsInputModel, TeamInterviewsView>>.WithNotEmptyValues,
                changeStatusFactory ?? Stub<IViewFactory<ChangeStatusInputModel, ChangeStatusView>>.WithNotEmptyValues,
                interviewDetailsFactory ?? Stub<IViewFactory<InterviewDetailsInputModel, InterviewDetailsView>>.WithNotEmptyValues,
                interviewSummaryViewFactory ?? Stub<IInterviewSummaryViewFactory>.WithNotEmptyValues);
        }
    }
}
