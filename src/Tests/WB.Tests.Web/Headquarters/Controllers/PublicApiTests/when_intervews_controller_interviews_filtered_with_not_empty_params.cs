using System;
using FluentAssertions;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.UI.Headquarters.API.PublicApi;
using WB.UI.Headquarters.API.PublicApi.Models;


namespace WB.Tests.Unit.Applications.Headquarters.PublicApiTests
{
    internal class when_intervews_controller_interviews_filtered_with_not_empty_params : ApiTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            allInterviewsViewFactory = new Mock<IAllInterviewsFactory>();
            controller = CreateInterviewsController(allInterviewsViewViewFactory : allInterviewsViewFactory.Object);

            BecauseOf();
        }

        public void BecauseOf() 
        {
            actionResult = controller.InterviewsFiltered(questionnaireId, questionnaireVersion);
        }

        [NUnit.Framework.Test] public void should_return_InterviewApiView () =>
            actionResult.Should().BeOfType<InterviewApiView>();

        [NUnit.Framework.Test] public void should_call_factory_load_once () =>
            allInterviewsViewFactory.Verify(x => x.Load(Moq.It.IsAny<AllInterviewsInputModel>()), Times.Once());

        private static InterviewApiView actionResult;
        private static InterviewsController controller;

        private static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static long questionnaireVersion = 1;

        private static Mock<IAllInterviewsFactory> allInterviewsViewFactory;
    }
}
