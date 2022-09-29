using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.RosterViewModelTests
{
    [TestFixture]
    internal class when_roster_instances_added : RosterViewModelTests
    {
        [Test]
        public async Task should_append_new_roster_instances()
        {
            var rosterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var chapterId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var rosterSizeQuestion = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            var interviewId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE").ToString("N");

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(chapterId,
                Create.Entity.MultyOptionsQuestion(rosterSizeQuestion,
                    new[]
                    {
                        Create.Entity.Answer("answer 1", 1), Create.Entity.Answer("answer 2", 2),
                        Create.Entity.Answer("answer 3", 3)
                    }),
                Create.Entity.Roster(rosterId, rosterSizeQuestionId: rosterSizeQuestion));

            var interview = SetUp.StatefulInterview(questionnaire);
            interview.AnswerMultipleOptionsQuestion(Guid.NewGuid(), rosterSizeQuestion, RosterVector.Empty,
                DateTime.UtcNow, new[] { 1 });

            var statefulInterviewRepository = Create.Fake.StatefulInterviewRepositoryWith(interview);
            var viewModel = this.CreateViewModel(interviewRepository: statefulInterviewRepository);
            var navigationState = Create.Other.NavigationState(statefulInterviewRepository);

            await navigationState.NavigateTo(Create.Entity.NavigationIdentity(Identity.Create(chapterId, RosterVector.Empty)));
            viewModel.Init(interviewId, Create.Entity.Identity(rosterId), navigationState);

            interview.AnswerMultipleOptionsQuestion(Guid.NewGuid(), rosterSizeQuestion, RosterVector.Empty,
                DateTime.UtcNow, new[] { 1, 3 });
            await viewModel.HandleAsync(Create.Event.RosterInstancesAdded(rosterId, new[] { Create.Entity.RosterVector(3) }));

            Assert.That(viewModel.RosterInstances.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task should_insert_item_inside_list()
        {
            var rosterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var chapterId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var rosterSizeQuestion = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            var interviewId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE").ToString("N");

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(chapterId,
                Create.Entity.MultyOptionsQuestion(rosterSizeQuestion,
                    new []
                    {
                        Create.Entity.Answer("answer 1", 1), Create.Entity.Answer("answer 2", 2),
                        Create.Entity.Answer("answer 3", 3)
                    }),
                Create.Entity.Roster(rosterId, rosterSizeQuestionId: rosterSizeQuestion));

            var interview = SetUp.StatefulInterview(questionnaire);
            interview.AnswerMultipleOptionsQuestion(Guid.NewGuid(), rosterSizeQuestion, RosterVector.Empty,
                DateTime.UtcNow, new[] {1, 3});

            var statefulInterviewRepository = Create.Fake.StatefulInterviewRepositoryWith(interview);
            var viewModel = this.CreateViewModel(interviewRepository: statefulInterviewRepository);
            var navigationState = Create.Other.NavigationState(statefulInterviewRepository);

            await navigationState.NavigateTo(Create.Entity.NavigationIdentity(Identity.Create(chapterId, RosterVector.Empty)));
            viewModel.Init(interviewId, Create.Entity.Identity(rosterId), navigationState);

            interview.AnswerMultipleOptionsQuestion(Guid.NewGuid(), rosterSizeQuestion, RosterVector.Empty,
                DateTime.UtcNow, new[] { 1, 2, 3 });

            await viewModel.HandleAsync(Create.Event.RosterInstancesAdded(rosterId, new[] { Create.Entity.RosterVector(2) }));

            Assert.That(viewModel.RosterInstances[1].Identity, Is.EqualTo(Identity.Create(rosterId, Create.Entity.RosterVector(2))));
        }
    }
}
