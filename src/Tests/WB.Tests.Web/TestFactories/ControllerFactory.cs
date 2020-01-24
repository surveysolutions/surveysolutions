using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using AutoMapper;
using Microsoft.Owin.Security;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Implementation;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Headquarters.Views.UsersAndQuestionnaires;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.Implementation;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Versions;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;
using WB.UI.Headquarters.API.DataCollection.Interviewer;
using WB.UI.Headquarters.Code.CommandTransformation;
using WB.UI.Headquarters.Controllers;
using WB.UI.Headquarters.Services;
using WB.UI.Shared.Web.Services;
using AssignmentsController = WB.UI.Headquarters.API.PublicApi.AssignmentsController;

namespace WB.Tests.Web.TestFactories
{
    internal class ControllerFactory
    {
        public Core.SharedKernels.SurveyManagement.Web.Controllers.AttachmentsController AttachmentsController(IAttachmentContentService attachmentContentService, IImageProcessingService imageProcessingService = null)
            => new Core.SharedKernels.SurveyManagement.Web.Controllers.AttachmentsController(attachmentContentService, imageProcessingService ?? Mock.Of<IImageProcessingService>());

        public ReportsController ReportsController(
            IMapReport mapReport = null,
            IAllUsersAndQuestionnairesFactory allUsersAndQuestionnairesFactory = null,
            IAuthorizedUser authorizedUser = null,
            IUserViewFactory userViewFactory = null,
            ITeamUsersAndQuestionnairesFactory teamUsersAndQuestionnairesFactory = null)
        {
            return new ReportsController(allUsersAndQuestionnairesFactory ?? Mock.Of<IAllUsersAndQuestionnairesFactory>(_ => _.Load() ==
                new AllUsersAndQuestionnairesView() { Questionnaires = new TemplateViewItem[0] }),
                authorizedUser ?? Mock.Of<IAuthorizedUser>(),
                userViewFactory ?? Mock.Of<IUserViewFactory>(),
                Mock.Of<IChartStatisticsViewFactory>(),
                new TestInMemoryWriter<InterviewSummary>(), null);
        }

        public InterviewerApiController InterviewerApiController(ITabletInformationService tabletInformationService = null,
            IUserViewFactory userViewFactory = null,
            IInterviewerSyncProtocolVersionProvider syncVersionProvider = null,
            IAuthorizedUser authorizedUser = null,
            HqSignInManager signInManager = null,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory = null,
            IAssignmentsService assignmentsService = null,
            IInterviewInformationFactory interviewInformationFactory = null,
            IPlainKeyValueStorage<InterviewerSettings> interviewerSettings = null,
            IPlainKeyValueStorage<TenantSettings> tenantSettings = null,
            IInterviewerVersionReader interviewerVersionReader = null)
        {
            var result = new InterviewerApiController(tabletInformationService ?? Mock.Of<ITabletInformationService>(),
                userViewFactory ?? Mock.Of<IUserViewFactory>(),
                syncVersionProvider ?? new InterviewerSyncProtocolVersionProvider(),
                authorizedUser ?? Mock.Of<IAuthorizedUser>(),
                signInManager ?? new HqSignInManager(Abc.Create.Storage.HqUserManager(), Mock.Of<IAuthenticationManager>(),
                Mock.Of<IHashCompatibilityProvider>()),
                questionnaireBrowseViewFactory ?? Mock.Of<IQuestionnaireBrowseViewFactory>(x =>
                    x.GetByIds(It.IsAny<QuestionnaireIdentity[]>()) == new List<QuestionnaireBrowseItem>()),
                interviewInformationFactory ?? Mock.Of<IInterviewInformationFactory>(),
                assignmentsService ?? Mock.Of<IAssignmentsService>(),
                Mock.Of<IClientApkProvider>(),
                interviewerSettings ?? Mock.Of<IPlainKeyValueStorage<InterviewerSettings>>(),
                tenantSettings ?? new InMemoryKeyValueStorage<TenantSettings>(),
                interviewerVersionReader ?? Mock.Of<IInterviewerVersionReader>()
            );
            result.Request = new HttpRequestMessage();
            result.Request.SetConfiguration(new HttpConfiguration());

            return result;
        }

        public AssignmentsController AssignmentsPublicApiController(
            IAssignmentViewFactory assignmentViewFactory = null,
            IAssignmentsService assignmentsService = null,
            IMapper mapper = null,
            HqUserManager userManager = null,
            IQuestionnaireStorage questionnaireStorage = null,
            ISystemLog auditLog = null,
            IInterviewCreatorFromAssignment interviewCreatorFromAssignment = null,
            IPreloadedDataVerifier verifier = null,
            ICommandTransformator commandTransformator = null,
            ICommandService commandService = null,
            IAuthorizedUser authorizedUser = null
            )
        {
            var result = new AssignmentsController(assignmentViewFactory,
                assignmentsService,
                mapper,
                userManager,
                Mock.Of<ILogger>(),
                questionnaireStorage,
                auditLog,
                interviewCreatorFromAssignment,
                verifier,
                commandTransformator,
                Abc.Create.Service.AssignmentFactory(),
                Mock.Of<IInvitationService>(),
                Mock.Of<IAssignmentPasswordGenerator>(),
                commandService ?? Mock.Of<ICommandService>(),
                authorizedUser ?? Mock.Of<IAuthorizedUser>()
                );
            result.Request = new HttpRequestMessage();
            result.Request.SetConfiguration(new HttpConfiguration());

            return result;
        }
    }
}
