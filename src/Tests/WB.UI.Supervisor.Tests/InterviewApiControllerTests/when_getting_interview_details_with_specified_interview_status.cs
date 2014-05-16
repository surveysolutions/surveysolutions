using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.View;
using Moq;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.UI.Supervisor.Controllers;
using WB.UI.Supervisor.Models;
using It = Machine.Specifications.It;

namespace WB.UI.Supervisor.Tests.InterviewApiControllerTests
{
    internal class when_getting_interview_details_with_specified_interview_status : InterviewApiControllerTestsContext
    {
        private Establish context = () =>
        {
            var interviewDetailsFactoryMock =
                new Mock<IViewFactory<InterviewDetailsInputModel, InterviewDetailsView>>();
            interviewDetailsFactoryMock.Setup(_ => _.Load(Moq.It.IsAny<InterviewDetailsInputModel>()))
                .Returns(
                    () =>
                        new InterviewDetailsView()
                        {
                            Status = verifiedStatus,
                            Responsible = new UserLight(new Guid(), "some user name")
                        });

            controller = CreateController(interviewDetailsFactory: interviewDetailsFactoryMock.Object);
        };

        Because of = () =>
            viewModel = controller.InterviewDetails(new InterviewDetailsViewModel() {InterviewId = new Guid()});

        It should_view_model_contains_localized_interview_status_from_resources = () =>
            viewModel.InterviewInfo.status.ShouldEqual(verifiedStatus.ToLocalizeString());

        private static InterviewApiController controller;
        private static NewInterviewDetailsView viewModel;
        private static InterviewStatus verifiedStatus = InterviewStatus.SupervisorAssigned;
    }
}
