﻿using System;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Web.Api;
using WB.Core.SharedKernels.SurveyManagement.Web.Models.Api;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Web.ApiTests
{
    internal class when_questionnaires_controller_interviews_with_not_empty_params : ApiTestContext
    {
        private Establish context = () =>
        {
            allInterviewsViewFactory = new Mock<IViewFactory<AllInterviewsInputModel, AllInterviewsView>>();
            controller = CreateQuestionnairesController(allInterviewsViewFactory: allInterviewsViewFactory.Object);
        };

        Because of = () =>
        {
            actionResult = controller.Interviews(questionnaireId, questionnaireVersion);
        };

        It should_return_InterviewApiView = () =>
            actionResult.ShouldBeOfExactType<InterviewApiView>();

        It should_call_factory_load_once = () =>
            allInterviewsViewFactory.Verify(x => x.Load(Moq.It.IsAny<AllInterviewsInputModel>()), Times.Once());

        private static InterviewApiView actionResult;
        private static QuestionnairesController controller;

        private static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static long questionnaireVersion = 1;

        private static Mock<IViewFactory<AllInterviewsInputModel, AllInterviewsView>> allInterviewsViewFactory;
    }

}
