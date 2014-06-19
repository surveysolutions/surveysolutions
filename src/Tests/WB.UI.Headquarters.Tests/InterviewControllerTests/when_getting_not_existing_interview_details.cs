using System;
using System.Web.Mvc;
using Machine.Specifications;
using Main.Core.View;
using Moq;
using WB.Core.SharedKernels.SurveyManagement.Views.ChangeStatus;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.UI.Headquarters.Controllers;
using It = Machine.Specifications.It;

namespace WB.UI.Headquarters.Tests.InterviewControllerTests
{
    internal class when_getting_not_existing_interview_details : InterviewControllerTestsContext
    {
        private Establish context = () =>
        {
            var changeStatusFactoryMock = new Mock<IViewFactory<ChangeStatusInputModel, ChangeStatusView>>();
            changeStatusFactoryMock.Setup(_ => _.Load(Moq.It.IsAny<ChangeStatusInputModel>()))
                .Returns((ChangeStatusView) null);

            controller = CreateController(changeStatusFactory: changeStatusFactoryMock.Object);
        };

        Because of = () =>
            actionResult = controller.InterviewDetails(new Guid(), string.Empty, null, null, null);

        It should_action_result_be_type_of_http_not_found_result = () =>
            actionResult.ShouldBeOfExactType<HttpNotFoundResult>();

        private static InterviewController controller;
        private static ActionResult actionResult;
    }
}
