using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Headquarters.Views.UsersAndQuestionnaires;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PeriodicStatusReportTests
{
    [Subject(typeof(PeriodicStatusReportController))]
    internal class PeriodicStatusReportControllerTestContext
    {
        protected static PeriodicStatusReportController CreatePeriodicStatusReportController(IAuthorizedUser authorizedUser = null)
        {
            return new PeriodicStatusReportController(Mock.Of<ICommandService>(),
                authorizedUser ?? Mock.Of<IAuthorizedUser>(), Mock.Of<ILogger>(),
                Mock.Of<IAllUsersAndQuestionnairesFactory>(
                    _ =>
                        _.Load(Moq.It.IsAny<AllUsersAndQuestionnairesInputModel>()) ==
                        new AllUsersAndQuestionnairesView() {Questionnaires = new TemplateViewItem[0]}), Mock.Of<IUserViewFactory>());
        }
    }
}
