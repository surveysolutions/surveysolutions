using System;
using Machine.Specifications;
using NSubstitute;
using WB.Core.BoundedContexts.Tester.Implementation.Aggregates;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Tests.Unit.BoundedContexts.Tester.ViewModels.NavigationStateTests
{
    [Subject(typeof (NavigationState))]
    public class when_navigating_back_to_removed_roster_instance
    {
        Establish context = () =>
        {
            rosterIdentity = Create.Identity(Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"), new[] {0m});
            interview = Substitute.For<IStatefulInterview>();
            interview.HasGroup(rosterIdentity)
                     .Returns(true);
            interview.IsEnabled(rosterIdentity)
                     .Returns(true);

            navigationState = Create.NavigationState(Setup.StatefulInterviewRepository(interview));
            navigationState.NavigateToAsync(rosterIdentity).WaitAndUnwrapException();

            emptyHistoryHandler = () => emptyHandlerCalled = true;
        };

        Because of = () =>
        {
            interview.HasGroup(rosterIdentity).Returns(false);
            navigationState.NavigateBackAsync(emptyHistoryHandler).WaitAndUnwrapException();
        };

        It should_skip_removed_group = () => emptyHandlerCalled.ShouldBeTrue();

        static NavigationState navigationState;
        static Identity rosterIdentity;
        static IStatefulInterview interview;
        static Action emptyHistoryHandler;
        static bool emptyHandlerCalled;
    }
}

