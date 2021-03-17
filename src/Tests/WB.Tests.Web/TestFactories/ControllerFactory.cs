using System.Net.Http;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Implementation;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Headquarters.Views.UsersAndQuestionnaires;
using WB.Core.BoundedContexts.Headquarters.Workspaces;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.Implementation;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Infrastructure.Native.Questionnaire;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Tests.Abc.Storage;
using WB.UI.Headquarters.Code.CommandTransformation;
using WB.UI.Headquarters.Controllers;
using WB.UI.Headquarters.Controllers.Api.DataCollection.Interviewer;
using WB.UI.Headquarters.Controllers.Services.Export;
using WB.UI.Headquarters.Services;
using AssignmentsController = WB.UI.Headquarters.Controllers.Api.PublicApi.AssignmentsController;

namespace WB.Tests.Web.TestFactories
{
    internal class ControllerFactory
    {
        public ReportsController ReportsController(
            IMapReport mapReport = null,
            IAllUsersAndQuestionnairesFactory allUsersAndQuestionnairesFactory = null,
            IAuthorizedUser authorizedUser = null,
            IUserViewFactory userViewFactory = null,
            ITeamUsersAndQuestionnairesFactory teamUsersAndQuestionnairesFactory = null)
        {
            var reportsController = new ReportsController(
                mapReport ?? Mock.Of<IMapReport>(),
                Mock.Of<IChartStatisticsViewFactory>(),
                allUsersAndQuestionnairesFactory ?? Mock.Of<IAllUsersAndQuestionnairesFactory>(_ => _.Load() == new AllUsersAndQuestionnairesView() { Questionnaires = new TemplateViewItem[0] }),
                new TestInMemoryWriter<InterviewSummary>(),
                userViewFactory ?? Mock.Of<IUserViewFactory>(),
                authorizedUser ?? Mock.Of<IAuthorizedUser>());

            reportsController.Url = Mock.Of<IUrlHelper>();

            return reportsController;
        }

        public InterviewerControllerBase InterviewerApiController(ITabletInformationService tabletInformationService = null,
            IUserViewFactory userViewFactory = null,
            IInterviewerSyncProtocolVersionProvider syncVersionProvider = null,
            IAuthorizedUser authorizedUser = null,
            IPlainKeyValueStorage<InterviewerSettings> interviewerSettings = null,
            IPlainStorageAccessor<ServerSettings> tenantSettings = null,
            IInterviewerVersionReader interviewerVersionReader = null,
            IUserToDeviceService userToDeviceService = null)
        {
            var result = new InterviewerControllerBase(
                tabletInformationService ?? Mock.Of<ITabletInformationService>(),
                userViewFactory ?? Mock.Of<IUserViewFactory>(),
                syncVersionProvider ?? new InterviewerSyncProtocolVersionProvider(),
                authorizedUser ?? Mock.Of<IAuthorizedUser>(),
                Mock.Of<IClientApkProvider>(),
                interviewerSettings ?? Mock.Of<IPlainKeyValueStorage<InterviewerSettings>>(),
                tenantSettings ?? new TestPlainStorage<ServerSettings>(),
                interviewerVersionReader ?? Mock.Of<IInterviewerVersionReader>(),
                userToDeviceService ?? Mock.Of<IUserToDeviceService>()
            );

            result.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            return result;
        }

        public AssignmentsController AssignmentsPublicApiController(
            IAssignmentViewFactory assignmentViewFactory = null,
            IAssignmentsService assignmentsService = null,
            IMapper mapper = null,
            IUserRepository userManager = null,
            IQuestionnaireStorage questionnaireStorage = null,
            ISystemLog auditLog = null,
            IPreloadedDataVerifier verifier = null,
            ICommandTransformator commandTransformator = null,
            ICommandService commandService = null,
            IAuthorizedUser authorizedUser = null,
            IUserViewFactory userViewFactory = null,
            IAssignmentsImportService assignmentsImportService = null,
            ISerializer serializer = null)
        {
            var sl = Mock.Of<IServiceLocator>(x => x.GetInstance<IAssignmentsService>() == assignmentsService);
            var scopeExecutor = Abc.Create.Service.InScopeExecutor(sl);
            
            var result = new AssignmentsController(
                assignmentViewFactory,
                assignmentsService,
                mapper,
                userManager,
                questionnaireStorage,
                auditLog,
                verifier,
                commandService ?? Mock.Of<ICommandService>(),
                authorizedUser ?? Mock.Of<IAuthorizedUser>(),
                Mock.Of<IUnitOfWork>(),
                userViewFactory ?? Mock.Of<IUserViewFactory>(),
                assignmentsImportService ?? Mock.Of<IAssignmentsImportService>(),
                serializer ?? Mock.Of<ISerializer>(),
                Mock.Of<IInvitationService>(),
                Mock.Of<IWebInterviewLinkProvider>(),
                scopeExecutor);

            return result;
        }

        public InstallController InstallController(ISupportedVersionProvider supportedVersionProvider = null,
            SignInManager<HqUser> identityManager = null,
            UserManager<HqUser> userManager = null,
            IUserRepository userRepository = null)
        {
            var result = new InstallController(
                supportedVersionProvider ?? Abc.Create.Service.SupportedVersionProvider(),
                identityManager ?? Create.Service.SignInManager(),
                userManager ?? Create.Service.UserManager(),
                userRepository);
            return result;
        }

        public QuestionnaireApiController QuestionnaireApiController(   IQuestionnaireStorage questionnaireStorage = null,
            ISerializer serializer = null,
            IPlainKeyValueStorage<QuestionnairePdf> pdfStorage = null,
            IReusableCategoriesStorage reusableCategoriesStorage = null,
            ITranslationStorage translationStorage = null,
            IQuestionnaireTranslator translator = null,
            IPlainKeyValueStorage<QuestionnaireBackup> questionnaireBackupStorage = null)
        {
            return new QuestionnaireApiController(
                questionnaireStorage ?? Mock.Of<IQuestionnaireStorage>(),
                serializer ?? Abc.Create.Service.NewtonJsonSerializer(),
                pdfStorage ?? new TestPlainStorage<QuestionnairePdf>(),
                reusableCategoriesStorage ?? Mock.Of<IReusableCategoriesStorage>(),
                translationStorage ?? Mock.Of<ITranslationStorage>(),
                translator ?? Mock.Of<IQuestionnaireTranslator>(),
                questionnaireBackupStorage ?? new TestPlainStorage<QuestionnaireBackup>()
            );
        }
    }
}
