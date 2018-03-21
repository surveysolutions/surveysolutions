using System;
using FluentAssertions;
using WB.Tests.Abc;
using WB.UI.Headquarters.API.PublicApi;
using WB.UI.Headquarters.API.PublicApi.Models;


namespace WB.Tests.Unit.Applications.Headquarters.PublicApiTests
{
    internal class when_intervews_controller_interview_details_with_not_empty_params : ApiTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var statefulInterview = Create.AggregateRoot.StatefulInterview(interviewId);

            controller = CreateInterviewsController(statefulInterviewRepository: Create.Fake.StatefulInterviewRepositoryWith(statefulInterview));
            BecauseOf();
        }

        public void BecauseOf() 
        {
            actionResult = controller.InterviewDetails(interviewId);
        }

        [NUnit.Framework.Test] public void should_return_InterviewApiDetails () =>
            actionResult.Should().BeOfType<InterviewApiDetails>();
        
        private static Guid interviewId = Guid.Parse("11111111111111111111111111111111");
        private static InterviewApiDetails actionResult;
        private static InterviewsController controller;
        
    }
}
