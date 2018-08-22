using System;
using System.Linq;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.RosterViewModelTests
{
    [TestFixture]
    internal class when_roster_instances_removed : RosterViewModelTests
    {
        [Test]
        public async Task should_remove_view_models()
        {
            var rosterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var chapterId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var textListQuestionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            var interviewerId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");


            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(chapterId,
                Create.Entity.TextListQuestion(textListQuestionId),
                Create.Entity.Roster(rosterId, rosterSizeSourceType: RosterSizeSourceType.Question, rosterSizeQuestionId: textListQuestionId));

            var interview = Setup.StatefulInterview(questionnaire);
            interview.AnswerTextListQuestion(interviewerId, textListQuestionId, RosterVector.Empty, DateTime.UtcNow,
                new[]
                {
                    new Tuple<decimal, string>(1, "option 1"),
                    new Tuple<decimal, string>(5, "option 5"),
                });

            var statefulInterviewRepository = Create.Fake.StatefulInterviewRepositoryWith(interview);
            var viewModel = this.CreateViewModel(interviewRepository: statefulInterviewRepository);
            var navigationState = Create.Other.NavigationState(statefulInterviewRepository);

            await navigationState.NavigateTo(Create.Entity.NavigationIdentity(Identity.Create(chapterId, RosterVector.Empty)));
            viewModel.Init(null, Create.Entity.Identity(rosterId), navigationState);

            interview.AnswerTextListQuestion(interviewerId, textListQuestionId, RosterVector.Empty, DateTime.UtcNow,
                new[]
                {
                    new Tuple<decimal, string>(5, "option 5"),
                });

            viewModel.Handle(Create.Event.RosterInstancesRemoved(rosterId, new [] { Create.Entity.RosterVector(1)}));

            Assert.That(viewModel.RosterInstances.Select(x => x.Identity).ToArray(),
                Is.EquivalentTo(new[] {Identity.Create(rosterId, Create.Entity.RosterVector(5))}));
        }
    }
}
