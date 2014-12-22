using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Tests.Unit.BoundedContexts.Capi.InterviewViewModelTests
{
    internal class when_synchronized_data_contains_filtered_combobox_question : InterviewViewModelTestContext
    {
        Establish context = () =>
        {
            filteredComboboxQuestionId = Guid.Parse("44444444444444444444444444444444");


            questionnarie = CreateQuestionnaireDocumentWithOneChapter(
                new SingleQuestion()
                {
                    PublicKey = filteredComboboxQuestionId,
                    Answers =
                        new List<Answer>
                        {
                            new Answer() { AnswerValue = "1", AnswerText = "1" },
                            new Answer() { AnswerValue = "2", AnswerText = "2" },
                            new Answer() { AnswerValue = "3", AnswerText = "3" }
                        },
                    QuestionType = QuestionType.SingleOption,
                    IsFilteredCombobox = true
                }
                );

            rosterStructure = CreateQuestionnaireRosterStructure(questionnarie);

            interviewSynchronizationDto = CreateInterviewSynchronizationDto(
                answers: new[]
                {
                    new AnsweredQuestionSynchronizationDto(filteredComboboxQuestionId, new decimal[0], 2.00m, null)
                },
                propagatedGroupInstanceCounts: new Dictionary<InterviewItemId, RosterSynchronizationDto[]>());
        };

        Because of = () =>
          interviewViewModel = CreateInterviewViewModel(questionnarie, rosterStructure, interviewSynchronizationDto);

        It should_answer_string_of_filtered_combobox_question_be_equal_to_2_1 = () =>
            ((FilteredComboboxQuestionViewModel)interviewViewModel.FindQuestion(
                question => question.PublicKey == new InterviewItemId(filteredComboboxQuestionId, new decimal[0]))
                .FirstOrDefault()).AnswerString.ShouldEqual("2");

        private static InterviewViewModel interviewViewModel;
        private static QuestionnaireDocument questionnarie;
        private static QuestionnaireRosterStructure rosterStructure;
        private static InterviewSynchronizationDto interviewSynchronizationDto;

        private static Guid filteredComboboxQuestionId;
    }
}
