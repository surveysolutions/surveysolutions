using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Hosting;
using Microsoft.Owin.Security;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Headquarters.Views.UsersAndQuestionnaires;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.Versions;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Tests.Abc.Storage;
using WB.UI.Headquarters.API.DataCollection;
using WB.UI.Headquarters.API.DataCollection.Interviewer;
using WB.UI.Headquarters.Controllers;

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
            return new ReportsController(mapReport ?? Mock.Of<IMapReport>(),
                allUsersAndQuestionnairesFactory ?? Mock.Of<IAllUsersAndQuestionnairesFactory>(_ => _.Load() ==
                new AllUsersAndQuestionnairesView() { Questionnaires = new TemplateViewItem[0] }),
                authorizedUser ?? Mock.Of<IAuthorizedUser>(),
                userViewFactory ?? Mock.Of<IUserViewFactory>(),
                teamUsersAndQuestionnairesFactory ?? Mock.Of<ITeamUsersAndQuestionnairesFactory>(),
                new TestInMemoryWriter<InterviewSummary>());
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
            IAssignmentsService assignmentsService = null)
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
                    signInManager ?? new HqSignInManager(Create.Storage.HqUserManager(), Mock.Of<IAuthenticationManager>()),
                    questionnaireBrowseViewFactory ??
                    Mock.Of<IQuestionnaireBrowseViewFactory>(x => x.GetByIds(It.IsAny<QuestionnaireIdentity[]>()) == new List<QuestionnaireBrowseItem>()),
                    assignmentsService ?? Mock.Of<IAssignmentsService>()
                );
                result.Request = new HttpRequestMessage();
                result.Request.SetConfiguration(new HttpConfiguration());

                return result;
            }
        }
    }
}
