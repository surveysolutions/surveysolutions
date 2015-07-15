using System;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Tester.Implementation.Aggregates;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.Core.SharedKernels.DataCollection;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Tester.ViewModels.NavigationStateTests
{
    public class when_navigating_to_not_existing_group
    {
        Establish context = () =>
        {
            var interview = Mock.Of<IStatefulInterview>(_
                => _.HasGroup(notExistingGroup) == false
                   && _.IsEnabled(notExistingGroup) == true);

            navigationState = Create.NavigationState(
                interviewRepository: Setup.StatefulInterviewRepository(interview));

            navigationState.GroupChanged += eventArgs => navigatedTo = eventArgs.TargetGroup;
        };

        Because of = () =>
            navigationState.NavigateToAsync(notExistingGroup);

        It should_not_navigate = () =>
            navigatedTo.ShouldBeNull();

        private static NavigationState navigationState;
        private static Identity notExistingGroup = Create.Identity(Guid.Parse("11111111111111111111111111111111"), Empty.RosterVector);
        private static Identity navigatedTo;
    }
}