﻿using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Web.Api;
using WB.Core.SharedKernels.SurveyManagement.Web.Models.Api;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Web.ApiTests
{
    internal class when_intervews_controller_interviews_filtered_with_empty_params : ApiTestContext
    {
        private Establish context = () =>
        {
            allInterviewsViewFactory = new Mock<IAllInterviewsFactory>();
            controller = CreateInterviewsController(allInterviewsViewViewFactory : allInterviewsViewFactory.Object);
        };

        Because of = () =>
        {
            actionResult = controller.InterviewsFiltered();
        };

        It should_return_InterviewApiView = () =>
            actionResult.ShouldBeOfExactType<InterviewApiView>();

        It should_call_factory_load_once = () =>
            allInterviewsViewFactory.Verify(x => x.Load(Moq.It.IsAny<AllInterviewsInputModel>()), Times.Once());

        private static InterviewApiView actionResult;
        private static InterviewsController controller;

        private static Mock<IAllInterviewsFactory> allInterviewsViewFactory;
    }
}
