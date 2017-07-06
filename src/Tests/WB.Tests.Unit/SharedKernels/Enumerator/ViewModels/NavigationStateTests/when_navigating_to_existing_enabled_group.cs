using System;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Tests.Abc;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.NavigationStateTests
{
    internal class when_navigating_to_existing_enabled_group
    {
        Establish context = () =>
        {
            var interview = Mock.Of<IStatefulInterview>(_
                => _.HasGroup(existingEnabledGroup) == true
                   && _.IsEnabled(existingEnabledGroup) == true);

            navigationState = Create.Other.NavigationState(
                interviewRepository: Setup.StatefulInterviewRepository(interview));

            navigationState.ScreenChanged += eventArgs => navigatedTo = eventArgs.TargetGroup;
        };

        Because of = () =>
            navigationState.NavigateTo(NavigationIdentity.CreateForGroup(existingEnabledGroup));

        It should_navigate_to_that_group = () =>
            navigatedTo.ShouldEqual(existingEnabledGroup);

        private static NavigationState navigationState;
        private static Identity existingEnabledGroup = Create.Entity.Identity(Guid.Parse("11111111111111111111111111111111"), Empty.RosterVector);
        private static Identity navigatedTo;
    }
}