using FluentAssertions;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.UI.Headquarters.API.PublicApi;
using WB.UI.Headquarters.API.PublicApi.Models;


namespace WB.Tests.Unit.Applications.Headquarters.PublicApiTests
{
    internal class when_intervews_controller_interviews_filtered_with_empty_params : ApiTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            allInterviewsViewFactory = new Mock<IAllInterviewsFactory>();
            controller = CreateInterviewsController(allInterviewsViewViewFactory : allInterviewsViewFactory.Object);
            BecauseOf();
        }

        public void BecauseOf() 
        {
            actionResult = controller.InterviewsFiltered();
        }

        [NUnit.Framework.Test] public void should_return_InterviewApiView () =>
            actionResult.Should().BeOfType<InterviewApiView>();

        [NUnit.Framework.Test] public void should_call_factory_load_once () =>
            allInterviewsViewFactory.Verify(x => x.Load(Moq.It.IsAny<AllInterviewsInputModel>()), Times.Once());

        private static InterviewApiView actionResult;
        private static InterviewsController controller;

        private static Mock<IAllInterviewsFactory> allInterviewsViewFactory;
    }
}
