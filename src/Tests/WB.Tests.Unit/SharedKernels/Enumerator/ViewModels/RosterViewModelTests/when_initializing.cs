using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Moq;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Aggregates;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.RosterViewModelTests
{
    [TestFixture]
    internal class when_initializing : RosterViewModelTests
    {
        public void should_read_roster_instances_from_interview()
        {
            var rosterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            var interview = new Mock<IStatefulInterview>();

            interview.Setup(x => x.GetRosterInstances(It.IsAny<Identity>(), rosterId))
                .Returns(new ReadOnlyCollection<Identity>(new List<Identity>
                {
                    Create.Entity.Identity(rosterId, Create.Entity.RosterVector(1)),
                    Create.Entity.Identity(rosterId, Create.Entity.RosterVector(2))
                }));

            var interviewRepository = Create.Fake.StatefulInterviewRepositoryWith(interview.Object);
            var viewModel = this.CreateViewModel(interviewRepository: interviewRepository);

            viewModel.Init("interviewId", Create.Entity.Identity(rosterId), Create.Other.NavigationState());

            Assert.That(viewModel.RosterInstances.Count(), Is.EqualTo(2));
        }
    }
}