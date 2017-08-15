using System;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Hosting;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Services.HealthCheck;
using WB.Core.BoundedContexts.Headquarters.Services.Internal;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.SurveyManagement.Web.Api;
using WB.Core.SharedKernels.SurveyManagement.Web.Api.Interviewer.v2;
using WB.Tests.Abc;
using WB.UI.Headquarters.API.PublicApi;
using WB.UI.Headquarters.Services;

namespace WB.Tests.Unit.Applications.Headquarters.PublicApiTests
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
            IUserViewFactory userViewViewFactory = null)
        {
            return new UsersController(
                logger ?? Mock.Of<ILogger>(),
                userViewViewFactory ?? Mock.Of<IUserViewFactory>(),
                null);
        }

        protected static QuestionnairesController CreateQuestionnairesController(
            ILogger logger = null,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewViewFactory = null,
            IAllInterviewsFactory allInterviewsViewFactory = null)
        {
            return new QuestionnairesController(
                logger ?? Mock.Of<ILogger>(),
                questionnaireBrowseViewViewFactory ?? Mock.Of<IQuestionnaireBrowseViewFactory>(),
                allInterviewsViewFactory ?? Mock.Of<IAllInterviewsFactory>(),
                serializer: Mock.Of<ISerializer>(),
                questionnaireStorage: Mock.Of<IQuestionnaireStorage>());
        }

        protected static InterviewsController CreateInterviewsController(
            ILogger logger = null,
            IAllInterviewsFactory allInterviewsViewViewFactory = null,
            IInterviewDetailsViewFactory interviewDetailsView = null,
            ICommandService commandService = null,
            IAuthorizedUser authorizedUser = null,
            IUserViewFactory userViewFactory = null,
            IReadSideKeyValueStorage<InterviewReferences> interviewReferences = null,
            IInterviewUniqueKeyGenerator interviewUniqueKeyGenerator = null,
            IPlainStorageAccessor<Assignment> assignmentStorageAccessor = null)
        {
            var controller = new InterviewsController(
                logger ?? Mock.Of<ILogger>(),
                allInterviewsViewViewFactory ?? Mock.Of<IAllInterviewsFactory>(),
                interviewDetailsView ?? Mock.Of<IInterviewDetailsViewFactory>(), Mock.Of<IInterviewHistoryFactory>(),
                commandService ?? Mock.Of<ICommandService>(),
                authorizedUser ?? Mock.Of<IAuthorizedUser>(),
                userViewFactory ?? Mock.Of<IUserViewFactory>(),
                interviewReferences ?? Mock.Of<IReadSideKeyValueStorage<InterviewReferences>>(),
                interviewUniqueKeyGenerator ?? Mock.Of<IInterviewUniqueKeyGenerator>(),
                assignmentStorageAccessor ?? Mock.Of<IPlainStorageAccessor<Assignment>>());

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
        protected static AttachmentsApiV2Controller CreateAttachmentsApiV2Controller(
            IAttachmentContentService attachmentContentService)
        {
            return new AttachmentsApiV2Controller(attachmentContentService ?? Mock.Of<IAttachmentContentService>());
        }

        protected static QuestionnairesApiV2Controller CreateQuestionnairesApiV2Controller(
            IQuestionnaireAssemblyAccessor questionnareAssemblyFileAccessor = null,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory = null,
            ISerializer serializer = null,
            IQuestionnaireStorage questionnaireStorage = null,
            IPlainStorageAccessor<QuestionnaireBrowseItem> readsideRepositoryWriter = null)
        {
            var questionnairesApiV2Controller = new QuestionnairesApiV2Controller(
                questionnareAssemblyFileAccessor ?? Mock.Of<IQuestionnaireAssemblyAccessor>(),
                questionnaireBrowseViewFactory ?? Mock.Of<IQuestionnaireBrowseViewFactory>(),
                serializer ?? Mock.Of<ISerializer>(),
                questionnaireStorage ?? Mock.Of<IQuestionnaireStorage>(),
                readsideRepositoryWriter ?? Mock.Of<IPlainStorageAccessor<QuestionnaireBrowseItem>>());

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://localhost");
            httpRequestMessage.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            questionnairesApiV2Controller.Request = httpRequestMessage;

            return questionnairesApiV2Controller;
        }
    }
}