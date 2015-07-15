using Machine.Specifications;

using Moq;

using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.Core.Infrastructure.EventBus.Lite;

using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Tester.ViewModels.ActiveGroupViewModelTests
{
    public class when_initializing_active_group_view_model : ActiveGroupViewModelTestContext
    {
        Establish context = () =>
        {
            activeGroup = CreateActiveGroupViewModel(eventRegistry: eventRegistry.Object);
        };

        Because of = () => activeGroup.Init(interviewId, navigationState.Object);

        It should_subscribe_view_model_for_events =
            () => eventRegistry.Verify(x => x.Subscribe(activeGroup, interviewId), Times.Once);

        static ActiveGroupViewModel activeGroup;

        static readonly string interviewId = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";

        static readonly Mock<NavigationState> navigationState = new Mock<NavigationState>();

        static Mock<ILiteEventRegistry> eventRegistry = new Mock<ILiteEventRegistry>();

        
    }
}