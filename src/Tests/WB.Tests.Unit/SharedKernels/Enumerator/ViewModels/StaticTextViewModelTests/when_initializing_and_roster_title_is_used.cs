using FluentAssertions;
using NUnit.Framework;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.StaticTextViewModelTests
{
    internal class when_initializing_and_roster_title_is_used : StaticTextViewModelTestsContext
    {
        [Test]
        public void should_substitute_roster_title_value()
        {
            var rosterTitleAnswerValue = "answer";
            var staticTextWithSubstitutionToRosterTitleId = Id.gB;

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(children:
                Create.Entity.FixedRoster(fixedTitles: new[] {new FixedRosterTitle(1, rosterTitleAnswerValue)},
                    children: new[]
                    {
                        Create.Entity.StaticText(publicKey: staticTextWithSubstitutionToRosterTitleId,
                            text: "uses %rostertitle%")
                    }));

            var questionnaireRepository = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire);
            var statefulInterview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire);
            var interviewRepository = Create.Fake.StatefulInterviewRepositoryWith(statefulInterview);

            var viewModel = CreateViewModel(questionnaireRepository, interviewRepository);

            // act
            viewModel.Init("interview",
                Create.Identity(staticTextWithSubstitutionToRosterTitleId, Create.Entity.RosterVector(1)),
                Create.Other.NavigationState());

            // assert
            viewModel.Text.PlainText.Should().Be($"uses {rosterTitleAnswerValue}");
        }
    }
}
