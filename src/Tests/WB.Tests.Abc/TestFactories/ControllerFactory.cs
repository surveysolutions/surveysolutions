using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using AutoMapper;
using Microsoft.Owin.Security;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.BoundedContexts.Headquarters.Factories;
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
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Versions;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc.Storage;
using WB.UI.Headquarters.API.DataCollection.Interviewer;
using WB.UI.Headquarters.Code.CommandTransformation;
using WB.UI.Headquarters.Controllers;
using AssignmentsController = WB.UI.Headquarters.API.PublicApi.AssignmentsController;

namespace WB.Tests.Abc.TestFactories
{
    internal class ControllerFactory
    {
        public Core.SharedKernels.SurveyManagement.Web.Controllers.AttachmentsController AttachmentsController(IAttachmentContentService attachmentContentService)
            => new WB.Core.SharedKernels.SurveyManagement.Web.Controllers.AttachmentsController(attachmentContentService);

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

        public InterviewerApiController InterviewerApiController(
            IFileSystemAccessor fileSystemAccessor = null,
            ITabletInformationService tabletInformationService = null,
            IUserViewFactory userViewFactory = null,
            IAndroidPackageReader androidPackageReader = null,
            IInterviewerSyncProtocolVersionProvider syncVersionProvider = null,
            IAuthorizedUser authorizedUser = null,
            IProductVersion productVersion = null,
            HqSignInManager signInManager = null,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory = null,
            IAssignmentsService assignmentsService = null,
            IPlainKeyValueStorage<InterviewerSettings> interviewerSettings = null)
        {
            {
                var result = new InterviewerApiController(
                    fileSystemAccessor ?? Mock.Of<IFileSystemAccessor>(),
                    tabletInformationService ?? Mock.Of<ITabletInformationService>(),
                    userViewFactory ?? Mock.Of<IUserViewFactory>(),
                    androidPackageReader ?? Mock.Of<IAndroidPackageReader>(),
                    syncVersionProvider ?? Mock.Of<IInterviewerSyncProtocolVersionProvider>(),
                    authorizedUser ?? Mock.Of<IAuthorizedUser>(),
                    productVersion ?? Mock.Of<IProductVersion>(),
                    signInManager ?? new HqSignInManager(Create.Storage.HqUserManager(), Mock.Of<IAuthenticationManager>(), Mock.Of<IHashCompatibilityProvider>()),
                    questionnaireBrowseViewFactory ?? Mock.Of<IQuestionnaireBrowseViewFactory>(x => x.GetByIds(It.IsAny<QuestionnaireIdentity[]>()) == new List<QuestionnaireBrowseItem>()),
                    assignmentsService ?? Mock.Of<IAssignmentsService>(),
                    interviewerSettings ?? Mock.Of<IPlainKeyValueStorage<InterviewerSettings>>()
                );
                result.Request = new HttpRequestMessage();
                result.Request.SetConfiguration(new HttpConfiguration());

                return result;
            }
        }

        public AssignmentsController AssignmentsPublicApiController(
            IAssignmentViewFactory assignmentViewFactory = null,
            IPlainStorageAccessor<Assignment> assignmentsStorage = null,
            IMapper mapper = null,
            HqUserManager userManager = null,
            IQuestionnaireStorage questionnaireStorage = null,
            IAuditLog auditLog = null,
            IInterviewCreatorFromAssignment interviewCreatorFromAssignment = null,
            IPreloadedDataVerifier verifier = null,
            ICommandTransformator commandTransformator = null
            )
        {
            var result = new AssignmentsController(assignmentViewFactory,
                assignmentsStorage,
                mapper,
                userManager,
                Mock.Of<ILogger>(),
                questionnaireStorage,
                auditLog,
                interviewCreatorFromAssignment,
                verifier,
                commandTransformator,
                Create.Service.AssignmentFactory(),
                Mock.Of<IInvitationService>());
            result.Request = new HttpRequestMessage();
            result.Request.SetConfiguration(new HttpConfiguration());

            return result;
        }
    }
}
