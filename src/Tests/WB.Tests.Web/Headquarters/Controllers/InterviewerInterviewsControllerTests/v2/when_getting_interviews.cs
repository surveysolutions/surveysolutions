using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Tests.Abc;
using WB.UI.Headquarters.Controllers.Api.DataCollection.Interviewer.v2;

namespace WB.Tests.Web.Headquarters.Controllers.InterviewerInterviewsControllerTests.v2
{
    internal class when_getting_interviews : InterviewsApiV2ControllerTestsContext
    {
        [Test]
        public void should_response_contains_interview_api_views_by_specified_interviewer()
        {
            var interviewInformationFactory = Mock.Of<IInterviewInformationFactory>(
                x => x.GetInProgressInterviewsForInterviewer(interviewerId) == interviewsFromStorage);

            controller = CreateInterviewerInterviewsController(
                authorizedUser: Mock.Of<IAuthorizedUser>(x => x.Id == interviewerId),
                interviewsFactory: interviewInformationFactory);
            
            controller.ControllerContext = new ControllerContext(new ActionContext(new DefaultHttpContext(), new RouteData(), new ControllerActionDescriptor()));
            controller.Request.Headers[HeaderNames.UserAgent] = "org.worldbank.solutions.interviewer/25.01 (build 33333) (QuestionnaireVersion/27.0.0)";

            // act
            response = controller.Get();

            // assert
            Assert.That(response.Value.Select(x => x.Id), Is.EquivalentTo(new[] {interviewId1, interviewId2}));
        }

        private static readonly Guid interviewId2 = Id.g3;
        private static readonly Guid interviewId1 = Id.g2;
        private static readonly Guid interviewerId = Id.g1;

        private static readonly List<InterviewInformation> interviewsFromStorage = new List<InterviewInformation>()
        {
            new InterviewInformation
            {
                Id = interviewId1,
                QuestionnaireIdentity = new QuestionnaireIdentity(Guid.NewGuid(), 1),
                IsRejected = false
            },
            new InterviewInformation
            {
                Id = interviewId2,
                QuestionnaireIdentity = new QuestionnaireIdentity(Guid.NewGuid(), 1),
                IsRejected = true
            },
        };

        private static InterviewsApiV2Controller controller;
        private static ActionResult<List<InterviewApiView>> response;
    }
}
