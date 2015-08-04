using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;
using WB.Core.SharedKernels.SurveyManagement.Views.UsersAndQuestionnaires;
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
                Mock.Of<IViewFactory<AllUsersAndQuestionnairesInputModel, AllUsersAndQuestionnairesView>>(
                    _ =>
                        _.Load(Moq.It.IsAny<AllUsersAndQuestionnairesInputModel>()) ==
                        new AllUsersAndQuestionnairesView() {Questionnaires = new TemplateViewItem[0]}));
        }
    }
}
