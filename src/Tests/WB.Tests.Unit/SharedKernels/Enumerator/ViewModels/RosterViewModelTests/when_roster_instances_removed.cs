using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Moq;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Aggregates;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.RosterViewModelTests
{
    [TestFixture]
    internal class when_roster_instances_removed : RosterViewModelTests
    {
        [Test]
        public void should_remove_view_models()
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
            viewModel.Handle(Create.Event.RosterInstancesRemoved(rosterId, new RosterVector[] { Create.Entity.RosterVector(1)}));

            Assert.That(viewModel.RosterInstances.Count, Is.EqualTo(1));
        }
    }
}