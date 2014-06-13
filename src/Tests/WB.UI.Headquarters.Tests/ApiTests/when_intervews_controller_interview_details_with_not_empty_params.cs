using System;
using Machine.Specifications;
using Main.Core.View;
using Moq;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.UI.Headquarters.API;
using WB.Core.SharedKernels.SurveyManagement.Web.Models.Api;
using It = Machine.Specifications.It;

namespace WB.UI.Headquarters.Tests.ApiTests
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
            actionResult.ShouldBeOfExactType<InterviewApiDetails>();
        
        private static Guid interviewId = Guid.Parse("11111111111111111111111111111111");
        private static InterviewApiDetails actionResult;
        private static InterviewsController controller;
        
    }
}
