using System;
using FluentAssertions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.StaticTextViewModelTests
{
    internal class when_initializing_and_roster_title_is_used : StaticTextViewModelTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            rosterTitleAnswerValue = "answer";
            staticTextWithSubstitutionToRosterTitleId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            var questionnaire= Create.Entity.QuestionnaireDocumentWithOneChapter(children:
                Create.Entity.FixedRoster(fixedTitles: new[] {new FixedRosterTitle(1, rosterTitleAnswerValue)}, children: new[]
                {
                    Create.Entity.StaticText(publicKey: staticTextWithSubstitutionToRosterTitleId, text: "uses %rostertitle%")
                }));

            var questionnaireRepository = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire);
            var statefulInterview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire);
            var interviewRepository = Create.Fake.StatefulInterviewRepositoryWith(statefulInterview);

            viewModel = CreateViewModel(questionnaireRepository, interviewRepository);
            BecauseOf();
        }

        public void BecauseOf() => 
            viewModel.Init("interview", Create.Identity(staticTextWithSubstitutionToRosterTitleId, Create.Entity.RosterVector(1)), Create.Other.NavigationState());

        [NUnit.Framework.Test] public void should_substitute_roster_title_value () => 
            viewModel.Text.PlainText.Should().Be($"uses {rosterTitleAnswerValue}");

        static StaticTextViewModel viewModel;
        static Guid staticTextWithSubstitutionToRosterTitleId;
        static string rosterTitleAnswerValue;
    }
}
