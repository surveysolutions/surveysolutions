using System;
using Moq;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire.BrowseItem;
using WB.Core.SharedKernels.SurveyManagement.Services.HealthCheck;
using WB.Core.SharedKernels.SurveyManagement.Views;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Interviewer;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;
using WB.Core.SharedKernels.SurveyManagement.Views.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Api;


namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Web.ApiTests
{
    internal class ApiTestContext
    {
        protected static UserView CreateUserView(Guid userId, string userName)
        {
            return new UserView() { PublicKey = userId, UserName = userName };
        }

        protected static DetailsViewModel CreateInterviewDetailsView(Guid interviewId)
        {
            return new DetailsViewModel() {InterviewDetails = new InterviewDetailsView() {PublicKey = interviewId}};
        }

        protected static UsersController CreateUsersController(
            ILogger logger = null,
            IViewFactory<InterviewersInputModel, InterviewersView> interviewersViewViewFactory = null,
            IUserListViewFactory userListViewViewFactory = null,
            IUserViewFactory userViewViewFactory = null)
        {
            return new UsersController(
                logger ?? Mock.Of<ILogger>(),
                interviewersViewViewFactory ?? Mock.Of<IViewFactory<InterviewersInputModel, InterviewersView>>(),
                userListViewViewFactory ?? Mock.Of<IUserListViewFactory>(),
                userViewViewFactory ?? Mock.Of<IUserViewFactory>());
        }

        protected static QuestionnairesController CreateQuestionnairesController(
            ILogger logger = null,
            IViewFactory<QuestionnaireBrowseInputModel, QuestionnaireBrowseView> questionnaireBrowseViewViewFactory = null,
            IViewFactory<QuestionnaireItemInputModel, QuestionnaireBrowseItem> questionnaireBrowseItemViewFactory = null
            )
        {
            return new QuestionnairesController(
                logger ?? Mock.Of<ILogger>(),
                questionnaireBrowseViewViewFactory ?? Mock.Of<IViewFactory<QuestionnaireBrowseInputModel, QuestionnaireBrowseView>>(),
                questionnaireBrowseItemViewFactory ?? Mock.Of<IViewFactory<QuestionnaireItemInputModel, QuestionnaireBrowseItem>>());
        }

        protected static InterviewsController CreateInterviewsController(
            ILogger logger = null,
            IViewFactory<AllInterviewsInputModel, AllInterviewsView> allInterviewsViewViewFactory = null,
            IInterviewDetailsViewFactory interviewDetailsView = null)
        {
            return new InterviewsController(
                logger ?? Mock.Of<ILogger>(),
                allInterviewsViewViewFactory ?? Mock.Of<IViewFactory<AllInterviewsInputModel, AllInterviewsView>>(),
                interviewDetailsView ?? Mock.Of<IInterviewDetailsViewFactory>(), Mock.Of<IInterviewHistoryFactory>());
        }

        protected static HealthCheckApiController CreateHealthCheckApiController(
            IHealthCheckService healthCheckService)
        {
            return new HealthCheckApiController(healthCheckService ?? Mock.Of<IHealthCheckService>());
        }
    }
}