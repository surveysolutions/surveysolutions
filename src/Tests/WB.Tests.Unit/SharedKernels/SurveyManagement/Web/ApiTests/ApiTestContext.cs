using System;
using Moq;
using WB.Core.GenericSubdomains.Portable.Services;
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
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewViewFactory = null,
            IViewFactory<QuestionnaireItemInputModel, QuestionnaireBrowseItem> questionnaireBrowseItemViewFactory = null
            )
        {
            return new QuestionnairesController(
                logger ?? Mock.Of<ILogger>(),
                questionnaireBrowseViewViewFactory ?? Mock.Of<IQuestionnaireBrowseViewFactory>(),
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
            /*KP-4929    IDatabaseHealthCheck databaseHealthCheck,
              IEventStoreHealthCheck eventStoreHealthCheck, 
              IBrokenSyncPackagesStorage brokenSyncPackagesStorage, 
            IChunkReader chunkReader, 
              IFolderPermissionChecker folderPermissionChecker,*/
            IHealthCheckService healthCheckService)
        {
            return new HealthCheckApiController(
                /*KP-4929    databaseHealthCheck ?? Mock.Of<IDatabaseHealthCheck>(),
                  eventStoreHealthCheck ?? Mock.Of<IEventStoreHealthCheck>(),
                  brokenSyncPackagesStorage ?? Mock.Of<IBrokenSyncPackagesStorage>(),
                chunkReader ?? Mock.Of<IChunkReader>(),
                folderPermissionChecker ?? Mock.Of<IFolderPermissionChecker>()*/healthCheckService ?? Mock.Of<IHealthCheckService>());
        }
    }
}