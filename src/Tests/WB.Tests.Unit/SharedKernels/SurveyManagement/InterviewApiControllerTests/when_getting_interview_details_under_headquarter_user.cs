using System;
using Machine.Specifications;
using Moq;
using Questionnaire.Core.Web.Helpers;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.InterviewApiControllerTests
{
    internal class when_getting_interview_details_under_headquarter_user : InterviewApiControllerTestsContext
    {
        Establish context = () =>
        {
            var globalInfoProvider = Mock.Of<IGlobalInfoProvider>(_
                => _.IsHeadquarter == true);

            controller = CreateController(globalInfoProvider: globalInfoProvider);
        };

        Because of = () =>
            result = controller.InterviewDetails(new InterviewDetailsViewModel { InterviewId = Guid.Parse("11111111111111111111111111111111") });

        It should_return_not_null_result = () =>
            result.ShouldNotBeNull();

        private static InterviewApiController controller;
        private static NewInterviewDetailsView result;
    }
}