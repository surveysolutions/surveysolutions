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
    internal class when_navigating_back_to_removed_roster_instance
    {
        [NUnit.Framework.Test]
        public async Task should_skip_removed_group()
        {
            rosterIdentity = Create.Entity.Identity(Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"), new[] { 0m });
            interview = Substitute.For<IStatefulInterview>();
            interview.HasGroup(rosterIdentity)
                     .Returns(true);
            interview.IsEnabled(rosterIdentity)
                     .Returns(true);

            navigationState = Create.Other.NavigationState(Setup.StatefulInterviewRepository(interview));
            await navigationState.NavigateTo(NavigationIdentity.CreateForGroup(rosterIdentity));

            emptyHistoryHandler = () => emptyHandlerCalled = true;

            // Act
            interview.HasGroup(rosterIdentity).Returns(false);
            navigationState.NavigateBack(emptyHistoryHandler);

            //Assert
            emptyHandlerCalled.Should().BeTrue();
        }

        static NavigationState navigationState;
        static Identity rosterIdentity;
        static IStatefulInterview interview;
        static Action emptyHistoryHandler;
        static bool emptyHandlerCalled;
    }
}

