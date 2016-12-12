using System;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.StaticTextViewModelTests
{
    internal class when_initializing_and_roster_title_is_used : StaticTextViewModelTestsContext
    {
        Establish context = () =>
        {
            rosterTitleAnswerValue = "answer";
            staticTextWithSubstitutionToRosterTitleId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            var questionnaire= Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(children:
                Create.Entity.FixedRoster(fixedTitles: new[] {new FixedRosterTitle(1, rosterTitleAnswerValue)}, children: new[]
                {
                    Create.Entity.StaticText(publicKey: staticTextWithSubstitutionToRosterTitleId, text: "uses %rostertitle%")
                })));

            var questionnaireRepository = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(Guid.NewGuid(), questionnaire);
            var statefulInterview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire);
            var interviewRepository = Create.Fake.StatefulInterviewRepositoryWith(statefulInterview);

            viewModel = CreateViewModel(questionnaireRepository, interviewRepository);
        };

        Because of = () => 
            viewModel.Init("interview", new Identity(staticTextWithSubstitutionToRosterTitleId, Create.Entity.RosterVector(1)), null);

        It should_substitute_roster_title_value = () => 
            viewModel.Text.PlainText.ShouldEqual($"uses {rosterTitleAnswerValue}");

        static StaticTextViewModel viewModel;
        static Guid staticTextWithSubstitutionToRosterTitleId;
        static string rosterTitleAnswerValue;
    }
}