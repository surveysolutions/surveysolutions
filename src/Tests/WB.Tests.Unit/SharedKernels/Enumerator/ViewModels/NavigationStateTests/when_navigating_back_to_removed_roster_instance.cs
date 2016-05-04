using System;
using Machine.Specifications;
using Nito.AsyncEx.Synchronous;
using NSubstitute;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.NavigationStateTests
{
    [Subject(typeof (NavigationState))]
    internal class when_navigating_back_to_removed_roster_instance
    {
        Establish context = () =>
        {
            rosterIdentity = Create.Identity(Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"), new[] {0m});
            interview = Substitute.For<IStatefulInterview>();
            interview.HasGroup(rosterIdentity)
                     .Returns(true);
            interview.IsEnabled(rosterIdentity)
                     .Returns(true);

            navigationState = Create.NavigationState(Setup.StatefulInterviewRepository(interview));
            navigationState.NavigateToAsync(NavigationIdentity.CreateForGroup(rosterIdentity)).WaitAndUnwrapException();

            emptyHistoryHandler = () => emptyHandlerCalled = true;
        };

        Because of = () =>
        {
            interview.HasGroup(rosterIdentity).Returns(false);
            navigationState.NavigateBackAsync(emptyHistoryHandler).WaitAndUnwrapException();
        };

        It should_skip_removed_group = () => emptyHandlerCalled.ShouldBeTrue();

        static NavigationState navigationState;
        static Identity rosterIdentity;
        static IStatefulInterview interview;
        static Action emptyHistoryHandler;
        static bool emptyHandlerCalled;
    }
}

