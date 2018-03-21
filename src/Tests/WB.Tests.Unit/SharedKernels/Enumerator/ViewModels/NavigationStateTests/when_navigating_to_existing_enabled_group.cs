using System;
using FluentAssertions;
using Moq;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.NavigationStateTests
{
    internal class when_navigating_to_existing_enabled_group
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var interview = Mock.Of<IStatefulInterview>(_
                => _.HasGroup(existingEnabledGroup) == true
                   && _.IsEnabled(existingEnabledGroup) == true);

            navigationState = Create.Other.NavigationState(
                interviewRepository: Setup.StatefulInterviewRepository(interview));

            navigationState.ScreenChanged += eventArgs => navigatedTo = eventArgs.TargetGroup;
            BecauseOf();
        }

        public void BecauseOf() =>
            navigationState.NavigateTo(NavigationIdentity.CreateForGroup(existingEnabledGroup));

        [NUnit.Framework.Test] public void should_navigate_to_that_group () =>
            navigatedTo.Should().Be(existingEnabledGroup);

        private static NavigationState navigationState;
        private static Identity existingEnabledGroup = Create.Entity.Identity(Guid.Parse("11111111111111111111111111111111"), Empty.RosterVector);
        private static Identity navigatedTo;
    }
}
