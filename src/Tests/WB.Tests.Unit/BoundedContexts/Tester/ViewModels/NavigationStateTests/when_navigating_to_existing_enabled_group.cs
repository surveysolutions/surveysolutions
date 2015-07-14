using System;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Aggregates;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;
using WB.Core.SharedKernels.DataCollection;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.QuestionnaireTester.ViewModels.NavigationStateTests
{
    public class when_navigating_to_existing_enabled_group
    {
        Establish context = () =>
        {
            var interview = Mock.Of<IStatefulInterview>(_
                => _.HasGroup(existingEnabledGroup) == true
                   && _.IsEnabled(existingEnabledGroup) == true);

            navigationState = Create.NavigationState(
                interviewRepository: Setup.StatefulInterviewRepository(interview));

            navigationState.GroupChanged += eventArgs => navigatedTo = eventArgs.TargetGroup;
        };

        Because of = () =>
            navigationState.NavigateTo(existingEnabledGroup);

        It should_navigate_to_that_group = () =>
            navigatedTo.ShouldEqual(existingEnabledGroup);

        private static NavigationState navigationState;
        private static Identity existingEnabledGroup = Create.Identity(Guid.Parse("11111111111111111111111111111111"), Empty.RosterVector);
        private static Identity navigatedTo;
    }
}