using System;
using System.Web.Mvc;
using Machine.Specifications;
using Main.Core.View;
using Moq;
using WB.Core.SharedKernels.SurveyManagement.Views.ChangeStatus;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.InterviewControllerTests
{
    internal class when_getting_not_existing_interview_details : InterviewControllerTestsContext
    {
        Establish context = () =>
        {
            var changeStatusFactory = Mock.Of<IViewFactory<ChangeStatusInputModel, ChangeStatusView>>(_
                => _.Load(it.IsAny<ChangeStatusInputModel>()) == null as ChangeStatusView);

            controller = CreateController(changeStatusFactory: changeStatusFactory);
        };

        Because of = () =>
            actionResult = controller.InterviewDetails(Guid.Parse("11111111111111111111111111111111"), string.Empty, null, null, null);

        It should_return_http_not_found_result = () =>
            actionResult.ShouldBeOfExactType<HttpNotFoundResult>();

        private static InterviewController controller;
        private static ActionResult actionResult;
    }
}
