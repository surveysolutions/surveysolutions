using System;
using System.Linq;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.RosterViewModelTests
{
    [TestFixture]
    internal class when_roster_instances_removed : RosterViewModelTests
    {
        [Test]
        public void should_remove_view_models()
        {
            var rosterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var chapterId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(chapterId,
                Create.Entity.FixedRoster(rosterId, fixedTitles:
                    new[] {Create.Entity.FixedTitle(1), Create.Entity.FixedTitle(5)}));

            var interview = Setup.StatefulInterview(questionnaire);

            var statefulInterviewRepository = Create.Fake.StatefulInterviewRepositoryWith(interview);
            var viewModel = this.CreateViewModel(interviewRepository: statefulInterviewRepository);
            var navigationState = Create.Other.NavigationState(statefulInterviewRepository);

            navigationState.NavigateTo(Create.Entity.NavigationIdentity(Identity.Create(chapterId, RosterVector.Empty)));
            viewModel.Init(null, Create.Entity.Identity(rosterId), navigationState);

            interview.Apply(Create.Event.RosterInstancesRemoved(rosterId, new[] { Create.Entity.RosterVector(1) }));
            viewModel.Handle(Create.Event.RosterInstancesRemoved(rosterId, new [] { Create.Entity.RosterVector(1)}));

            Assert.That(viewModel.RosterInstances.Select(x => x.Identity).ToArray(),
                Is.EquivalentTo(new[] {Identity.Create(rosterId, Create.Entity.RosterVector(5))}));
        }
    }
}