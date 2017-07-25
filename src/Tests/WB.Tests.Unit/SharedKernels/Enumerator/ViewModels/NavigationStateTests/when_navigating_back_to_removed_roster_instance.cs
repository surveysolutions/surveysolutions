using System;
using Machine.Specifications;
using NSubstitute;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.NavigationStateTests
{
    [Subject(typeof (NavigationState))]
    internal class when_navigating_back_to_removed_roster_instance
    {
        Establish context = () =>
        {
            rosterIdentity = Create.Entity.Identity(Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"), new[] {0m});
            interview = Substitute.For<IStatefulInterview>();
            interview.HasGroup(rosterIdentity)
                     .Returns(true);
            interview.IsEnabled(rosterIdentity)
                     .Returns(true);

            navigationState = Create.Other.NavigationState(Setup.StatefulInterviewRepository(interview));
            navigationState.NavigateTo(NavigationIdentity.CreateForGroup(rosterIdentity));

            emptyHistoryHandler = () => emptyHandlerCalled = true;
        };

        Because of = () =>
        {
            interview.HasGroup(rosterIdentity).Returns(false);
            navigationState.NavigateBack(emptyHistoryHandler);
        };

        It should_skip_removed_group = () => emptyHandlerCalled.ShouldBeTrue();

        static NavigationState navigationState;
        static Identity rosterIdentity;
        static IStatefulInterview interview;
        static Action emptyHistoryHandler;
        static bool emptyHandlerCalled;
    }
}

