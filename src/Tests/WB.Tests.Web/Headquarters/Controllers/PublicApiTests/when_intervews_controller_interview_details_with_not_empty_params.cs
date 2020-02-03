using System;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using WB.Tests.Abc;
using WB.UI.Headquarters.API.PublicApi.Models;
using WB.UI.Headquarters.Controllers.Api.PublicApi;

namespace WB.Tests.Unit.Applications.Headquarters.PublicApiTests
{
    internal class when_intervews_controller_interview_details_with_not_empty_params : ApiTestContext
    {
        [Test]
        public void should_return_InterviewApiDetails()
        {
            var statefulInterview = Create.AggregateRoot.StatefulInterview(interviewId);

            controller = CreateInterviewsController(statefulInterviewRepository: Create.Fake.StatefulInterviewRepositoryWith(statefulInterview));
            BecauseOf();

            Assert.That(actionResult.Value.Answers, Is.Not.Null);
        }

        public void BecauseOf()
        {
            actionResult = controller.Get(interviewId);
        }

        private static Guid interviewId = Guid.Parse("11111111111111111111111111111111");
        private static ActionResult<InterviewApiDetails> actionResult;
        private static InterviewsPublicApiController controller;

    }
}
