using System;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.SurveyManagement.Views;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.InterviewApiControllerTests
{
    internal class when_getting_interview_details_under_headquarter_user : InterviewApiControllerTestsContext
    {
        Establish context = () =>
        {
            var globalInfoProvider = Mock.Of<IGlobalInfoProvider>(_
                => _.IsHeadquarter == true);

            var interviewDetailsViewFactory =
                Mock.Of<IInterviewDetailsViewFactory>(
                    x =>
                        x.GetInterviewDetails(Moq.It.IsAny<Guid>(), Moq.It.IsAny<Guid?>(), Moq.It.IsAny<decimal[]>(),
                            Moq.It.IsAny<InterviewDetailsFilter?>()) ==
                        new DetailsViewModel() {InterviewDetails = new InterviewDetailsView()});

            controller = CreateController(globalInfoProvider: globalInfoProvider, interviewDetailsFactory: interviewDetailsViewFactory);
        };

        Because of = () =>
            result = controller.InterviewDetails(new InterviewDetailsViewModel { InterviewId = Guid.Parse("11111111111111111111111111111111") });

        It should_return_not_null_result = () =>
            result.ShouldNotBeNull();

        private static InterviewApiController controller;
        private static NewInterviewDetailsView result;
    }
}