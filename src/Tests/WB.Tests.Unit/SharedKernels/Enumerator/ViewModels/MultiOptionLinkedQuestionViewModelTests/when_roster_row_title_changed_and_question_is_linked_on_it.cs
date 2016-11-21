using System;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.MultiOptionLinkedQuestionViewModelTests
{
    internal class when_roster_row_title_changed_and_question_is_linked_on_it : MultiOptionLinkedQuestionViewModelTestsContext
    {
        Establish context = () =>
        {
            questionId = new Identity(Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"), RosterVector.Empty);
            rosterId = new Identity(Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB"), RosterVector.Empty);

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.FixedRoster(rosterId.Id, fixedTitles: new[] {new FixedRosterTitle(1, "title")}),
                Create.Entity.MultyOptionsQuestion(questionId.Id, linkedToRosterId: rosterId.Id)
                );

            interview = Setup.StatefulInterview(questionnaire);

            var questionnaireRepository = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire);
            var interviewRepository = Create.Fake.StatefulInterviewRepositoryWith(interview);

            viewModel = CreateMultiOptionRosterLinkedQuestionViewModel(interviewRepository: interviewRepository, questionnaireStorage: questionnaireRepository);
            viewModel.Init("interview", questionId, Create.Other.NavigationState());
        };

        Because of = () =>
        {
            viewModel.Handle(Create.Event.RosterInstancesTitleChanged(rosterId: rosterId.Id, rosterTitle: "title",
                outerRosterVector: rosterId.RosterVector, instanceId: 1));
        };

        It should_insert_new_option = () => viewModel.Options.Count.ShouldEqual(1);

        static MultiOptionLinkedToRosterQuestionViewModel viewModel;
        static StatefulInterview interview;
        static Identity questionId;
        static Identity rosterId;
    }
}

