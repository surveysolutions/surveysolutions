using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Hosting;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.SurveyManagement.Web.Api.Interviewer.v1;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Web.ApiTests.InterviewerInterviewsControllerTests.v1
{
    internal class when_getting_interviews : InterviewsApiV1ControllerTestsContext
    {
        private Establish context = () =>
        {
            var mockOfGlobalInfoProvider = new Mock<IGlobalInfoProvider>();
            mockOfGlobalInfoProvider.Setup(x => x.GetCurrentUser()).Returns(new UserLight(interviewerId, "interviewer"));

            var interviewInformationFactory = Mock.Of<IInterviewInformationFactory>(
                x => x.GetInProgressInterviews(interviewId1) == interviewsFromStorage);

            controller = CreateInterviewerInterviewsController(
                globalInfoProvider: mockOfGlobalInfoProvider.Object,
                interviewsFactory: interviewInformationFactory);
            controller.Request = new HttpRequestMessage();
            controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        };

        Because of = () => response = controller.Get();

        It should_response_contains_interview_api_views_by_specified_interviewer = () =>
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
        private static InterviewsApiV1Controller controller;
        private static HttpResponseMessage response;
    }
}
