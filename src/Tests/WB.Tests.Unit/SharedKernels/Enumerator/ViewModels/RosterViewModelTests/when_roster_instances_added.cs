using System;
using System.Linq;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.RosterViewModelTests
{
    [TestFixture]
    internal class when_roster_instances_added : RosterViewModelTests
    {
        [Test]
        public void should_append_new_roster_instances()
        {
            var rosterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var chapterId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var rosterSizeQuestion = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(chapterId,
                Create.Entity.MultyOptionsQuestion(rosterSizeQuestion,
                    new[]
                    {
                        Create.Entity.Answer("answer 1", 1), Create.Entity.Answer("answer 2", 2),
                        Create.Entity.Answer("answer 3", 3)
                    }),
                Create.Entity.Roster(rosterId, rosterSizeQuestionId: rosterSizeQuestion));

            var interview = Setup.StatefulInterview(questionnaire);
            interview.AnswerMultipleOptionsQuestion(Guid.NewGuid(), rosterSizeQuestion, RosterVector.Empty,
                DateTime.UtcNow, new[] { 1 });

            var statefulInterviewRepository = Create.Fake.StatefulInterviewRepositoryWith(interview);
            var viewModel = this.CreateViewModel(interviewRepository: statefulInterviewRepository);
            var navigationState = Create.Other.NavigationState(statefulInterviewRepository);

            navigationState.NavigateTo(Create.Entity.NavigationIdentity(Identity.Create(chapterId, RosterVector.Empty)));
            viewModel.Init(null, Create.Entity.Identity(rosterId), navigationState);

            interview.AnswerMultipleOptionsQuestion(Guid.NewGuid(), rosterSizeQuestion, RosterVector.Empty,
                DateTime.UtcNow, new[] { 1, 3 });
            viewModel.Handle(Create.Event.RosterInstancesAdded(rosterId, new[] { Create.Entity.RosterVector(3) }));

            Assert.That(viewModel.RosterInstances.Count(), Is.EqualTo(2));
        }

        [Test]
        public void should_insert_item_inside_list()
        {
            var rosterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var chapterId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var rosterSizeQuestion = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(chapterId,
                Create.Entity.MultyOptionsQuestion(rosterSizeQuestion,
                    new []
                    {
                        Create.Entity.Answer("answer 1", 1), Create.Entity.Answer("answer 2", 2),
                        Create.Entity.Answer("answer 3", 3)
                    }),
                Create.Entity.Roster(rosterId, rosterSizeQuestionId: rosterSizeQuestion));

            var interview = Setup.StatefulInterview(questionnaire);
            interview.AnswerMultipleOptionsQuestion(Guid.NewGuid(), rosterSizeQuestion, RosterVector.Empty,
                DateTime.UtcNow, new[] {1, 3});

            var statefulInterviewRepository = Create.Fake.StatefulInterviewRepositoryWith(interview);
            var viewModel = this.CreateViewModel(interviewRepository: statefulInterviewRepository);
            var navigationState = Create.Other.NavigationState(statefulInterviewRepository);

            navigationState.NavigateTo(Create.Entity.NavigationIdentity(Identity.Create(chapterId, RosterVector.Empty)));
            viewModel.Init(null, Create.Entity.Identity(rosterId), navigationState);

            interview.AnswerMultipleOptionsQuestion(Guid.NewGuid(), rosterSizeQuestion, RosterVector.Empty,
                DateTime.UtcNow, new[] { 1, 2, 3 });

            viewModel.Handle(Create.Event.RosterInstancesAdded(rosterId, new[] { Create.Entity.RosterVector(2) }));

            Assert.That(viewModel.RosterInstances[1].Identity, Is.EqualTo(Identity.Create(rosterId, Create.Entity.RosterVector(2))));
        }
    }
}