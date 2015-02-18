using System;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.SurveyManagement.Views;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Web.Api;
using WB.Core.SharedKernels.SurveyManagement.Web.Models.Api;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Web.ApiTests
{
    internal class when_intervews_controller_interview_details_with_not_empty_params : ApiTestContext
    {
        private Establish context = () =>
        {
            var allInterviewsViewFactory =
                Mock.Of<IInterviewDetailsViewFactory>(x => x.GetInterviewDetails(Moq.It.IsAny<Guid>(), Moq.It.IsAny<Guid?>(), Moq.It.IsAny<decimal[]>(), Moq.It.IsAny<InterviewDetailsFilter?>()) == CreateInterviewDetailsView(interviewId));

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
