using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Moq;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.RosterViewModelTests
{
    [TestFixture]
    internal class when_roster_instances_added : RosterViewModelTests
    {
        [Test]
        public void should_append_new_roster_instances()
        {
            var rosterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var interview = new Mock<IStatefulInterview>();
            interview.Setup(x => x.GetRosterInstances(It.IsAny<Identity>(), rosterId))
                .Returns(new List<Identity>().ToReadOnlyCollection());

            var interviewRepository = Create.Fake.StatefulInterviewRepositoryWith(interview.Object);

            var viewModel = this.CreateViewModel(interviewRepository: interviewRepository);
            viewModel.Init("interviewId", Create.Entity.Identity(rosterId), Create.Other.NavigationState());

            viewModel.Handle(Create.Event.RosterInstancesAdded(rosterId, Create.Entity.RosterVector(1), Create.Entity.RosterVector(2)));

            Assert.That(viewModel.RosterInstances.Count, Is.EqualTo(2));
        }

        [Test]
        public void should_insert_item_inside_list()
        {
            var rosterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            var interview = new Mock<IStatefulInterview>();

            interview.Setup(x => x.GetRosterInstances(It.IsAny<Identity>(), rosterId))
                .Returns(new ReadOnlyCollection<Identity>(new List<Identity>
                {
                    Create.Entity.Identity(rosterId, Create.Entity.RosterVector(1)),
                    Create.Entity.Identity(rosterId, Create.Entity.RosterVector(3))
                }));

            var interviewRepository = Create.Fake.StatefulInterviewRepositoryWith(interview.Object);

            var viewModel = this.CreateViewModel(interviewRepository: interviewRepository);
            viewModel.Init("interviewId", Create.Entity.Identity(rosterId), Create.Other.NavigationState());
            var addedRosterRosterVector = Create.Entity.RosterVector(2);
            viewModel.Handle(Create.Event.RosterInstancesAdded(rosterId, addedRosterRosterVector));

            Assert.That(viewModel.RosterInstances.Count, Is.EqualTo(3));
            var groupViewModel = viewModel.RosterInstances[1] as GroupViewModel;
            Assert.That(groupViewModel.Identity, Is.EqualTo(Create.Entity.Identity(rosterId, addedRosterRosterVector)));
        }
    }
}