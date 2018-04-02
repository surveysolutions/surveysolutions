using System;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.NavigationStateTests
{
    internal class when_navigating_back_to_complete_screen
    {
        [NUnit.Framework.OneTimeSetUp] public async Task context () {
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
            await navigationState.NavigateTo(NavigationIdentity.CreateForGroup(section1Identity));
            await navigationState.NavigateTo(NavigationIdentity.CreateForCompleteScreen());
            await navigationState.NavigateTo(NavigationIdentity.CreateForGroup(section2Identity));

            emptyHistoryHandler = () => { };

            navigationState.ScreenChanged += eventArgs => navigatedTo = eventArgs;
            BecauseOf();
        }

        public void BecauseOf() 
        {
            navigationState.NavigateBack(emptyHistoryHandler);
        }

        [NUnit.Framework.Test] public void should_navigate_to_complete_screen () =>
            navigatedTo.TargetStage.Should().Be(ScreenType.Complete);

        static NavigationState navigationState;
        static Identity section1Identity;
        static Identity section2Identity;
        static IStatefulInterview interview;
        static Action emptyHistoryHandler;
        
        static ScreenChangedEventArgs navigatedTo;
    }
}

