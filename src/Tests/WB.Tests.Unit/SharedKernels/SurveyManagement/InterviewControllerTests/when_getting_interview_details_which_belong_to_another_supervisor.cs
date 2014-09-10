using System;
using System.Web.Mvc;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.InterviewControllerTests
{
    internal class when_getting_interview_details_which_belong_to_another_supervisor : InterviewControllerTestsContext
    {
        Establish context = () =>
        {
            var thisSupervisorId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var anotherSupervisorId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            var globalInfoProvider = Mock.Of<IGlobalInfoProvider>(_
                => _.IsSurepvisor == true
                && _.GetCurrentUser().Id == thisSupervisorId);

            var interviewSummaryViewFactory = Mock.Of<IInterviewSummaryViewFactory>(_
                => _.Load(interviewId).ResponsibleId == anotherSupervisorId);

            controller = CreateController(
                globalInfoProvider: globalInfoProvider,
                interviewSummaryViewFactory: interviewSummaryViewFactory);
        };

        Because of = () =>
            actionResult = controller.InterviewDetails(interviewId, string.Empty, null, null, null);

        It should_return_http_not_found_result = () =>
            actionResult.ShouldBeOfExactType<HttpNotFoundResult>();

        private static InterviewController controller;
        private static ActionResult actionResult;
        private static Guid interviewId = Guid.Parse("11111111111111111111111111111111");
    }
}