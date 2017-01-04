using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Headquarters.Views.UsersAndQuestionnaires;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PeriodicStatusReportTests
{
    [Subject(typeof(PeriodicStatusReportController))]
    internal class PeriodicStatusReportControllerTestContext
    {
        protected static PeriodicStatusReportController CreatePeriodicStatusReportController(IGlobalInfoProvider globalInfoProvider=null)
        {
            return new PeriodicStatusReportController(Mock.Of<ICommandService>(),
                globalInfoProvider ?? Mock.Of<IGlobalInfoProvider>(), Mock.Of<ILogger>(),
                Mock.Of<IAllUsersAndQuestionnairesFactory>(
                    _ =>
                        _.Load(Moq.It.IsAny<AllUsersAndQuestionnairesInputModel>()) ==
                        new AllUsersAndQuestionnairesView() {Questionnaires = new TemplateViewItem[0]}), Mock.Of<IUserViewFactory>());
        }
    }
}
