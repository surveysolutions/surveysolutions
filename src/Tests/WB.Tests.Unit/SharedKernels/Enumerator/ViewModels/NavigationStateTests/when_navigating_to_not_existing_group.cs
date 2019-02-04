using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.NavigationStateTests
{
    internal class when_navigating_to_not_existing_group
    {
        [NUnit.Framework.OneTimeSetUp] public async Task context () {
            var interview = Mock.Of<IStatefulInterview>(_
                => _.HasGroup(notExistingGroup) == false
                   && _.IsEnabled(notExistingGroup) == true);

            navigationState = Create.Other.NavigationState(
                interviewRepository: SetUp.StatefulInterviewRepository(interview));

            navigationState.ScreenChanged += eventArgs => navigatedTo = eventArgs.TargetGroup;
            await BecauseOf();
        }

        public async Task BecauseOf() =>
            await navigationState.NavigateTo(NavigationIdentity.CreateForGroup(notExistingGroup));

        [NUnit.Framework.Test] public void should_not_navigate () =>
            navigatedTo.Should().BeNull();

        private static NavigationState navigationState;
        private static Identity notExistingGroup = Create.Entity.Identity(Guid.Parse("11111111111111111111111111111111"), Empty.RosterVector);
        private static Identity navigatedTo;
    }
}
