using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Hosting;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.UI.Headquarters.API.DataCollection.Interviewer.v2;


namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Web.ApiTests.InterviewerInterviewsControllerTests.v2
{
    internal class when_getting_interviews : InterviewsApiV2ControllerTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var interviewInformationFactory = Mock.Of<IInterviewInformationFactory>(
                x => x.GetInProgressInterviewsForInterviewer(interviewId1) == interviewsFromStorage);

            controller = CreateInterviewerInterviewsController(
                interviewsFactory: interviewInformationFactory);
            controller.Request = new HttpRequestMessage();
            controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            BecauseOf();
        }

        public void BecauseOf() => response = controller.Get();

        [NUnit.Framework.Test] public void should_response_contains_interview_api_views_by_specified_interviewer () =>
            GetTypedResponse<List<InterviewApiView>>().All(x => x.Id == interviewId1 || x.Id == interviewId2);

        private static T GetTypedResponse<T>() where T: class
        {
            return (T)((ObjectContent) response.Content).Value;
        }

        private static readonly Guid interviewId2 = Guid.Parse("33333333333333333333333333333333");
        private static readonly Guid interviewId1 = Guid.Parse("22222222222222222222222222222222");
        private static readonly Guid interviewerId = Guid.Parse("11111111111111111111111111111111");

        private static readonly InterviewInformation[] interviewsFromStorage =
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
        private static HttpResponseMessage response;
    }
}
