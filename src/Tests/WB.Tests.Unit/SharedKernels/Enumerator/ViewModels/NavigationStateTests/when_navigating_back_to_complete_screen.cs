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
    internal class when_navigating_back_to_complete_screen
    {
        Establish context = () =>
        {
            section1Identity = Create.Entity.Identity(Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"), Empty.RosterVector);
            section2Identity = Create.Entity.Identity(Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAB"), Empty.RosterVector);

            interview = Substitute.For<IStatefulInterview>();
            interview.HasGroup(section1Identity)
                        .Returns(true);
            interview.IsEnabled(section1Identity)
                     .Returns(true);

            interview.HasGroup(section2Identity)
                        .Returns(true);
            interview.IsEnabled(section2Identity)
                     .Returns(true);

            navigationState = Create.Other.NavigationState(Setup.StatefulInterviewRepository(interview));
            navigationState.NavigateToAsync(NavigationIdentity.CreateForGroup(section1Identity)).WaitAndUnwrapException();
            navigationState.NavigateToAsync(NavigationIdentity.CreateForCompleteScreen()).WaitAndUnwrapException();
            navigationState.NavigateToAsync(NavigationIdentity.CreateForGroup(section2Identity)).WaitAndUnwrapException();

            emptyHistoryHandler = () => { };

            navigationState.ScreenChanged += eventArgs => navigatedTo = eventArgs;
        };

        Because of = () =>
        {
            navigationState.NavigateBackAsync(emptyHistoryHandler).WaitAndUnwrapException();
        };

        It should_navigate_to_complete_screen = () =>
            navigatedTo.TargetScreen.ShouldEqual(ScreenType.Complete);

        static NavigationState navigationState;
        static Identity section1Identity;
        static Identity section2Identity;
        static IStatefulInterview interview;
        static Action emptyHistoryHandler;
        
        static ScreenChangedEventArgs navigatedTo;
    }
}

