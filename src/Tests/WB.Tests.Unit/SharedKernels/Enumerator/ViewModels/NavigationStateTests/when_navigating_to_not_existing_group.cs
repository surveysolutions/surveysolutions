using System;
using Machine.Specifications;
using Moq;
using Nito.AsyncEx.Synchronous;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.NavigationStateTests
{
    internal class when_navigating_to_not_existing_group
    {
        Establish context = () =>
        {
            var interview = Mock.Of<IStatefulInterview>(_
                => _.HasGroup(notExistingGroup) == false
                   && _.IsEnabled(notExistingGroup) == true);

            navigationState = Create.NavigationState(
                interviewRepository: Setup.StatefulInterviewRepository(interview));

            navigationState.ScreenChanged += eventArgs => navigatedTo = eventArgs.TargetGroup;
        };

        Because of = () =>
            navigationState.NavigateToAsync(NavigationIdentity.CreateForGroup(notExistingGroup)).WaitAndUnwrapException();

        It should_not_navigate = () =>
            navigatedTo.ShouldBeNull();

        private static NavigationState navigationState;
        private static Identity notExistingGroup = Create.Identity(Guid.Parse("11111111111111111111111111111111"), Empty.RosterVector);
        private static Identity navigatedTo;
    }
}