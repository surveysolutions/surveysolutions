using System;
using Moq;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Services.HealthCheck;
using WB.Core.SharedKernels.SurveyManagement.Views;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Interviewer;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;
using WB.Core.SharedKernels.SurveyManagement.Views.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Api;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using System.Net.Http;
using System.Web.Http;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;

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
            IInterviewersViewFactory interviewersViewViewFactory = null,
            IUserListViewFactory userListViewViewFactory = null,
            IUserViewFactory userViewViewFactory = null)
        {
            return new UsersController(
                logger ?? Mock.Of<ILogger>(),
                interviewersViewViewFactory ?? Mock.Of<IInterviewersViewFactory>(),
                userListViewViewFactory ?? Mock.Of<IUserListViewFactory>(),
                userViewViewFactory ?? Mock.Of<IUserViewFactory>());
        }

        protected static QuestionnairesController CreateQuestionnairesController(
            ILogger logger = null,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewViewFactory = null,
            IViewFactory<QuestionnaireItemInputModel, QuestionnaireBrowseItem> questionnaireBrowseItemViewFactory = null,
            IViewFactory<AllInterviewsInputModel, AllInterviewsView> allInterviewsViewFactory = null)
        {
            return new QuestionnairesController(
                logger ?? Mock.Of<ILogger>(),
                questionnaireBrowseViewViewFactory ?? Mock.Of<IQuestionnaireBrowseViewFactory>(),
                questionnaireBrowseItemViewFactory ?? Mock.Of<IViewFactory<QuestionnaireItemInputModel, QuestionnaireBrowseItem>>(),
                allInterviewsViewFactory ?? Mock.Of<IViewFactory<AllInterviewsInputModel, AllInterviewsView>>());
        }

        protected static InterviewsController CreateInterviewsController(
            ILogger logger = null,
            IViewFactory<AllInterviewsInputModel, AllInterviewsView> allInterviewsViewViewFactory = null,
            IInterviewDetailsViewFactory interviewDetailsView = null,
            ICommandService commandService = null,
            IGlobalInfoProvider globalInfoProvider = null,
            IUserViewFactory userViewFactory = null,
            IReadSideKeyValueStorage<InterviewReferences> interviewReferences = null)
        {
            var controller = new InterviewsController(
                logger ?? Mock.Of<ILogger>(),
                allInterviewsViewViewFactory ?? Mock.Of<IViewFactory<AllInterviewsInputModel, AllInterviewsView>>(),
                interviewDetailsView ?? Mock.Of<IInterviewDetailsViewFactory>(), Mock.Of<IInterviewHistoryFactory>(),
                commandService ?? Mock.Of<ICommandService>(),
                globalInfoProvider ?? Mock.Of<IGlobalInfoProvider>(),
                userViewFactory ?? Mock.Of<IUserViewFactory>(),
                interviewReferences ?? Mock.Of<IReadSideKeyValueStorage<InterviewReferences>>());

            controller.Request = new HttpRequestMessage(HttpMethod.Post, "https://localhost");
            controller.Request.SetConfiguration(new HttpConfiguration());

            return controller;
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