using System;
using System.Web.Mvc;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.SurveyManagement.Views;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.InterviewControllerTests
{
    internal class when_getting_interview_details_without_filter : InterviewControllerTestsContext
    {
        Establish context = () =>
        {
            var interviewDetailsFactory = Mock.Of<IInterviewDetailsViewFactory>(_ => _.GetFirstChapterId(Moq.It.IsAny<Guid>()) == firstQuestionnaireChapterId);
            
            controller = CreateController(interviewDetailsViewFactory: interviewDetailsFactory);
        };

        Because of = () =>
            actionResult = controller.Details(id: interviewId, filter: null, currentGroupId: null, rosterVector: null);

        It should_return_redirect_to_route_result = () =>
            actionResult.ShouldBeOfExactType<RedirectToRouteResult>();

        It should_result_contains_route_value_with_interviewid_ = () =>
            ((RedirectToRouteResult)actionResult).RouteValues["id"].ShouldEqual(interviewId);

        It should_result_contains_route_value_with_filter_by_all_groups_ = () =>
            ((RedirectToRouteResult)actionResult).RouteValues["filter"].ShouldEqual(InterviewDetailsFilter.All);

        It should_result_contains_route_value_with_firstQuestionnaireChapterId_ = () =>
            ((RedirectToRouteResult)actionResult).RouteValues["currentGroupId"].ShouldEqual(firstQuestionnaireChapterId);

        private static InterviewController controller;
        private static ActionResult actionResult;
        private static Guid interviewId = Guid.Parse("11111111111111111111111111111111");
        private static Guid firstQuestionnaireChapterId = Guid.Parse("22222222222222222222222222222222");
    }
}
