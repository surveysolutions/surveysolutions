using System;
using Machine.Specifications;
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
            navigationState.NavigateTo(NavigationIdentity.CreateForGroup(section1Identity));
            navigationState.NavigateTo(NavigationIdentity.CreateForCompleteScreen());
            navigationState.NavigateTo(NavigationIdentity.CreateForGroup(section2Identity));

            emptyHistoryHandler = () => { };

            navigationState.ScreenChanged += eventArgs => navigatedTo = eventArgs;
        };

        Because of = () =>
        {
            navigationState.NavigateBack(emptyHistoryHandler);
        };

        It should_navigate_to_complete_screen = () =>
            navigatedTo.TargetStage.ShouldEqual(ScreenType.Complete);

        static NavigationState navigationState;
        static Identity section1Identity;
        static Identity section2Identity;
        static IStatefulInterview interview;
        static Action emptyHistoryHandler;
        
        static ScreenChangedEventArgs navigatedTo;
    }
}

