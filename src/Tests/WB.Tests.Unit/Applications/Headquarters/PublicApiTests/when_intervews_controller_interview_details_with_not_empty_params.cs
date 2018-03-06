using System;
using Machine.Specifications;
using WB.Tests.Abc;
using WB.UI.Headquarters.API.PublicApi;
using WB.UI.Headquarters.API.PublicApi.Models;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.Headquarters.PublicApiTests
{
    internal class when_intervews_controller_interview_details_with_not_empty_params : ApiTestContext
    {
        private Establish context = () =>
        {
            var statefulInterview = Create.AggregateRoot.StatefulInterview(interviewId);

            controller = CreateInterviewsController(statefulInterviewRepository: Create.Fake.StatefulInterviewRepositoryWith(statefulInterview));
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
