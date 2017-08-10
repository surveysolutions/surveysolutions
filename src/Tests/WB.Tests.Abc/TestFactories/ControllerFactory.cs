using Moq;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Headquarters.Views.UsersAndQuestionnaires;
using WB.Tests.Abc.Storage;
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
                new TestInMemoryWriter<InterviewStatuses>());
        }
    }
}
