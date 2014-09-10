using System;
using System.Net;
using System.Web.Http;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.InterviewApiControllerTests
{
    internal class when_getting_interview_details_which_belong_to_another_supervisor : InterviewApiControllerTestsContext
    {
        Establish context = () =>
        {
            var thisSupervisorId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var anotherSupervisorId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            var globalInfoProvider = Mock.Of<IGlobalInfoProvider>(_
                => _.IsSurepvisor == true
                    && _.GetCurrentUser().Id == thisSupervisorId);

            var interviewSummaryViewFactory = Mock.Of<IInterviewSummaryViewFactory>(_
                => _.Load(interviewId).ResponsibleId == anotherSupervisorId);

            controller = CreateController(
                globalInfoProvider: globalInfoProvider,
                interviewSummaryViewFactory: interviewSummaryViewFactory);
        };

        Because of = () =>
            httpResponseException = Catch.Only<HttpResponseException>(() =>
                controller.InterviewDetails(new InterviewDetailsViewModel { InterviewId = interviewId }));

        It should_throw_HttpResponseException = () =>
            httpResponseException.ShouldNotBeNull();

        It should_throw_HttpResponseException_with_forbidden_status_code = () =>
            httpResponseException.Response.StatusCode.ShouldEqual(HttpStatusCode.Forbidden);

        private static InterviewApiController controller;
        private static Guid interviewId = Guid.Parse("11111111111111111111111111111111");
        private static HttpResponseException httpResponseException;
    }
}