using System;
using System.ServiceModel.Syndication;
using Moq;
using WB.Core.BoundedContexts.Headquarters.CalendarEvents;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.PdfInterview;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Users.UserProfile.InterviewerAuditLog;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.Implementation;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Workspaces;
using WB.Tests.Abc.Storage;
using WB.Tests.Web;
using WB.UI.Headquarters.API.WebInterview;
using WB.UI.Headquarters.Controllers.Api.DataCollection.Interviewer.v2;
using WB.UI.Headquarters.Controllers.Api.PublicApi;
using UsersController = WB.UI.Headquarters.Controllers.Api.PublicApi.UsersController;

namespace WB.Tests.Unit.Applications.Headquarters.PublicApiTests
{
    internal class ApiTestContext
    {
        protected static UserView CreateUserView(Guid userId, string userName)
        {
            return new UserView() { PublicKey = userId, UserName = userName };
        }

        protected static UsersController CreateUsersController(
            ILogger logger = null,
            IUserViewFactory userViewViewFactory = null)
        {
            return new UsersController(
                userViewViewFactory ?? Mock.Of<IUserViewFactory>(),
                Mock.Of<IUserArchiveService>(),
                Mock.Of<IAuditLogService>(),
                Create.Service.UserManager(),
                Mock.Of<IUnitOfWork>(),
                Mock.Of<ISystemLog>(),
                Mock.Of<IWorkspaceContextAccessor>(),
                Mock.Of<IPlainStorageAccessor<Workspace>>());
        }

        protected static QuestionnairesPublicApiController CreateQuestionnairesController(
            ILogger logger = null,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewViewFactory = null,
            IAllInterviewsFactory allInterviewsViewFactory = null,
            IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItems = null)
        {
            var questionnairesController = new QuestionnairesPublicApiController(
                questionnaireBrowseViewViewFactory ?? Mock.Of<IQuestionnaireBrowseViewFactory>(),
                allInterviewsViewFactory ?? Mock.Of<IAllInterviewsFactory>(),
                serializer: Mock.Of<ISerializer>(),
                questionnaireStorage: Mock.Of<IQuestionnaireStorage>(),
                questionnaireBrowseItems ?? new InMemoryPlainStorageAccessor<QuestionnaireBrowseItem>());
            return questionnairesController;
        }

        protected static InterviewsPublicApiController CreateInterviewsController(
            ILogger logger = null,
            IAllInterviewsFactory allInterviewsViewViewFactory = null,
            ICommandService commandService = null,
            IAuthorizedUser authorizedUser = null,
            IUserViewFactory userViewFactory = null,
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewReferences = null,
            IStatefulInterviewRepository statefulInterviewRepository = null,
            IStatefullInterviewSearcher statefullInterviewSearcher = null,
            IQuestionnaireStorage questionnaireStorage = null,
            IPdfInterviewGenerator pdfInterviewGenerator = null,
            ICalendarEventService calendarEventService = null)
        {
            var controller = new InterviewsPublicApiController(
                allInterviewsViewFactory: allInterviewsViewViewFactory ?? Mock.Of<IAllInterviewsFactory>(), 
                interviewHistoryViewFactory: Mock.Of<IInterviewHistoryFactory>(),
                userViewFactory: userViewFactory ?? Mock.Of<IUserViewFactory>(),
                interviewReferences: interviewReferences ?? new TestInMemoryWriter<InterviewSummary>(),
                statefulInterviewRepository: statefulInterviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                questionnaireStorage: questionnaireStorage ?? Mock.Of<IQuestionnaireStorage>(),
                commandService: commandService ?? Mock.Of<ICommandService>(),
                authorizedUser: authorizedUser ?? Mock.Of<IAuthorizedUser>(),
                Mock.Of<Microsoft.Extensions.Logging.ILogger<InterviewsPublicApiController>>(),
                statefullInterviewSearcher: statefullInterviewSearcher ?? Mock.Of<IStatefullInterviewSearcher>(),
                diagnosticsFactory: Mock.Of<IInterviewDiagnosticsFactory>(),
                pdfInterviewGenerator: pdfInterviewGenerator ?? Mock.Of<IPdfInterviewGenerator>(),
                calendarEventService: calendarEventService ?? Mock.Of<ICalendarEventService>()
                );

            return controller;
        }

        protected static AttachmentsApiV2Controller CreateAttachmentsApiV2Controller(
            IAttachmentContentService attachmentContentService)
        {
            var res = new AttachmentsApiV2Controller(attachmentContentService ?? Mock.Of<IAttachmentContentService>());
            return res;
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

            return questionnairesApiV2Controller;
        }
    }
}
