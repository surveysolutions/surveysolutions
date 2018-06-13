using System;
using System.Linq;
using FluentAssertions;
using Main.Core.Entities.Composite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.SingleOptionRosterLinkedQuestionViewModelTests
{
    internal class when_question_is_linked_to_second_level_question_and_title_changes : SingleOptionRosterLinkedQuestionViewModelTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var linkToRosterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var questionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            parentRosterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            var questionIdentity = Create.Entity.Identity(questionId, RosterVector.Empty);

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[] {
                Create.Entity.FixedRoster(rosterId: parentRosterId, variable: "f1", fixedTitles: new [] { Create.Entity.FixedTitle(1, "Level1")}, children: new IComposite[] {
                    Create.Entity.FixedRoster(rosterId: linkToRosterId, variable: "f2", fixedTitles: new [] { Create.Entity.FixedTitle(1, "Level2")})
                }),
                Create.Entity.SingleOptionQuestion(questionId: questionId, variable: "q1", linkedToRosterId: linkToRosterId)
            });

            var interview = Setup.StatefulInterview(questionnaire);

            var questionnaireRepository = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire.PublicKey,
                Create.Entity.PlainQuestionnaire(questionnaire));
            var interviewRepository = Create.Fake.StatefulInterviewRepositoryWith(interview);

            viewModel = CreateViewModel(interviewRepository, questionnaireRepository);
            viewModel.Init("interview", questionIdentity, Create.Other.NavigationState(interviewRepository));
            BecauseOf();
        }

        public void BecauseOf() => 
            viewModel.Handle(Create.Event.RosterInstancesTitleChanged(parentRosterId, Create.Entity.RosterVector(1, 2)));

        [NUnit.Framework.Test] public void should_refresh_roster_titles_in_options () => 
            viewModel.Options.Select(x => x.Title).Should().BeEquivalentTo("Level1: Level2");

        static SingleOptionRosterLinkedQuestionViewModel viewModel;
        static Guid parentRosterId;
    }
}
