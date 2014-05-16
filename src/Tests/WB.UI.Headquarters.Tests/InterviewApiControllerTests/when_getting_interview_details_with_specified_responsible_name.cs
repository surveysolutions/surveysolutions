using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.View;
using Moq;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.UI.Headquarters.Controllers;
using WB.UI.Headquarters.Models;
using It = Machine.Specifications.It;

namespace WB.UI.Headquarters.Tests.InterviewApiControllerTests
{
    internal class when_getting_interview_details_with_specified_responsible_name : InterviewApiControllerTestsContext
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
                            Responsible = new UserLight(new Guid(), responsibleUserName)
                        });

            controller = CreateController(interviewDetailsFactory: interviewDetailsFactoryMock.Object);
        };

        Because of = () =>
            viewModel = controller.InterviewDetails(new InterviewDetailsViewModel() {InterviewId = new Guid()});

        It should_view_model_contains_responsibleUserName_in_responsible_field = () =>
            viewModel.InterviewInfo.responsible.ShouldEqual(responsibleUserName);

        private static InterviewApiController controller;
        private static NewInterviewDetailsView viewModel;
        private static string responsibleUserName = "some user";
    }
}
