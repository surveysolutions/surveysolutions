using System;
using Core.Supervisor.Views.Interview;
using Machine.Specifications;
using Main.Core.View;
using Moq;
using Web.Supervisor.API;
using Web.Supervisor.Models.API;
using It = Machine.Specifications.It;

namespace Web.Supervisor.Tests.ApiTests
{
    internal class when_intervews_controller_interview_details_with_not_empty_params : ApiTestContext
    {
        private Establish context = () =>
        {
            var allInterviewsViewFactory =
                Mock.Of<IViewFactory<InterviewDetailsInputModel, InterviewDetailsView>>(x => x.Load(Moq.It.IsAny<InterviewDetailsInputModel>()) == CreateInterviewDetailsView(interviewId)); ;

            controller = CreateInterviewsController(interviewDetailsView :allInterviewsViewFactory);
        };

        Because of = () =>
        {
            actionResult = controller.InterviewDetails(interviewId);
        };

        It should_return_InterviewApiDetails = () =>
            actionResult.ShouldBeOfType<InterviewApiDetails>();

        It should_return_view_with_correct_id = () =>
            actionResult.Interview.PublicKey.ShouldEqual(interviewId);

        private static Guid interviewId = Guid.Parse("11111111111111111111111111111111");
        private static InterviewApiDetails actionResult;
        private static InterviewsController controller;
        
    }
}
