using Machine.Specifications;
using Main.Core.View;
using Moq;
using Ncqrs.Commanding.ServiceModel;
using Questionnaire.Core.Web.Helpers;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernels.SurveyManagement.Views.ChangeStatus;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Interviews;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;

namespace WB.UI.Headquarters.Tests.InterviewApiControllerTests
{
    [Subject(typeof(InterviewApiController))]
    internal class InterviewApiControllerTestsContext
    {
        protected static InterviewApiController CreateController(ICommandService commandService = null,
            IGlobalInfoProvider globalInfo = null, ILogger logger = null,
            IViewFactory<AllInterviewsInputModel, AllInterviewsView> allInterviewsViewFactory = null,
            IViewFactory<TeamInterviewsInputModel, TeamInterviewsView> teamInterviewViewFactory = null,
            IViewFactory<ChangeStatusInputModel, ChangeStatusView> changeStatusFactory = null,
            IViewFactory<InterviewDetailsInputModel, InterviewDetailsView> interviewDetailsFactory = null,
            IInterviewSummaryViewFactory interviewSummaryViewFactory = null)
        {
            return new InterviewApiController(commandService: commandService ?? Mock.Of<ICommandService>(),
                globalInfo: globalInfo ?? Mock.Of<IGlobalInfoProvider>(), logger: logger ?? Mock.Of<ILogger>(),
                allInterviewsViewFactory:
                    allInterviewsViewFactory ?? Mock.Of<IViewFactory<AllInterviewsInputModel, AllInterviewsView>>(),
                teamInterviewViewFactory:
                    teamInterviewViewFactory ?? Mock.Of<IViewFactory<TeamInterviewsInputModel, TeamInterviewsView>>(),
                changeStatusFactory:
                    changeStatusFactory ?? Mock.Of<IViewFactory<ChangeStatusInputModel, ChangeStatusView>>(),
                interviewDetailsFactory:
                    interviewDetailsFactory ?? Mock.Of<IViewFactory<InterviewDetailsInputModel, InterviewDetailsView>>(),
                interviewSummaryViewFactory: interviewSummaryViewFactory ?? Mock.Of<IInterviewSummaryViewFactory>());
        }
    }
}
