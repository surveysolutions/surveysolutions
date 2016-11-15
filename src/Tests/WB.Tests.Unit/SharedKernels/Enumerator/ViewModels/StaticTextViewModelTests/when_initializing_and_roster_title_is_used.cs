using System;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.StaticTextViewModelTests
{
    [Ignore("KP-8159")]
    internal class when_initializing_and_roster_title_is_used : StaticTextViewModelTestsContext
    {
        Establish context = () =>
        {
            rosterTitleAnswerValue = "answer";
            staticTextWithSubstitutionToRosterTitleId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            var questionnaire= Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocument(children:
                Create.Entity.FixedRoster(
                    fixedRosterTitles: new[] {new FixedRosterTitle(1, rosterTitleAnswerValue)}, children: new[]
                    {
                        Create.Entity.StaticText(publicKey: staticTextWithSubstitutionToRosterTitleId,
                            text: "uses %rostertitle%")
                    })));

            var questionnaireRepository = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(Guid.NewGuid(), questionnaire);
            var interviewRepository = Create.Fake.StatefulInterviewRepositoryWith(Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire));

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