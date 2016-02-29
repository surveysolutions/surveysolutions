using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using It = Machine.Specifications.It;


namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.EnumerationStageViewModelTests
{
    internal class when_initializing_active_group_view_model
    {
        Establish context = () =>
        {
            interviewRepositoryMock.Setup(x => x.Get(Moq.It.IsAny<string>())).Returns(Mock.Of<IStatefulInterview>());
            enumerationStage = Create.EnumerationStageViewModel(eventRegistry: eventRegistry.Object, interviewRepository: interviewRepositoryMock.Object);
        };

        Because of = () => enumerationStage.Init(interviewId, navigationState.Object, null, null);

        It should_subscribe_view_model_for_events =
            () => eventRegistry.Verify(x => x.Subscribe(enumerationStage, interviewId), Times.Once);

        static EnumerationStageViewModel enumerationStage;

        static readonly string interviewId = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";

        static readonly Mock<NavigationState> navigationState = new Mock<NavigationState>();

        static readonly Mock<ILiteEventRegistry> eventRegistry = new Mock<ILiteEventRegistry>();
        static readonly Mock<IStatefulInterviewRepository> interviewRepositoryMock = new Mock<IStatefulInterviewRepository>();
    }
}