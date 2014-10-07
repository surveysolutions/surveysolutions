using System;
using System.Web.Mvc;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.InterviewControllerTests
{
    internal class when_getting_interview_details_under_headquarter_user : InterviewControllerTestsContext
    {
        Establish context = () =>
        {
            var globalInfoProvider = Mock.Of<IGlobalInfoProvider>(_
                => _.IsHeadquarter == true);

            controller = CreateController(globalInfoProvider: globalInfoProvider);
        };

        Because of = () =>
            actionResult = controller.InterviewDetails(Guid.Parse("11111111111111111111111111111111"), string.Empty, null, null, null);

        It should_return_view_result = () =>
            actionResult.ShouldBeOfExactType<ViewResult>();

        private static InterviewController controller;
        private static ActionResult actionResult;
    }
}