using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Capi.Tests.Views.InterviewViewModelTests
{
    internal class when_adding_new_roster_screen_with_question_that_reference_prefilled_one_in_title : InterviewViewModelTestContext
    {
        Establish context = () =>
        {
            questionnarie = CreateQuestionnaireDocumentWithOneChapter(
                new NumericQuestion
                {
                    PublicKey = prefilledNumeric,
                    QuestionType = QuestionType.Numeric,
                    StataExportCaption = "numeric",
                    Featured = true
                },
                new Group
                {
                    PublicKey = firstLevelRosterId,
                    IsRoster = true,
                    RosterSizeQuestionId = prefilledNumeric,
                    Children = new List<IComposite>
                    {
                        new NumericQuestion
                        {
                            PublicKey = questionInRosterReferencePrefilledNumericId,
                            QuestionText = "Example %numeric%",
                            QuestionType = QuestionType.Numeric
                        }
                    }
                });

            rosterStructure = CreateQuestionnaireRosterStructure(questionnarie);

            interviewSynchronizationDto = CreateInterviewSynchronizationDto(
                answers: new[]
                {
                    new AnsweredQuestionSynchronizationDto(prefilledNumeric, new decimal[0], 2, string.Empty),
                },
                propagatedGroupInstanceCounts: new Dictionary<InterviewItemId, RosterSynchronizationDto[]>{});
            interviewViewModel = CreateInterviewViewModel(questionnarie, rosterStructure, interviewSynchronizationDto);
        };

        private Because of = () =>
             PropagateScreen(interviewViewModel, firstLevelRosterId, 0, new decimal[0]);

        It should_substitute_title_of_question_in_first_roster_row_with_answer_on_prefilled_question = () =>
            GetQuestion(questionInRosterReferencePrefilledNumericId, new decimal[] { 0 }).Text.ShouldEqual("Example 2");

        private static QuestionViewModel GetQuestion(Guid questionId, decimal[] rosterVector)
        {
            return interviewViewModel.FindQuestion(q => q.PublicKey == Create.InterviewItemId(questionId, rosterVector)).FirstOrDefault();
        }

        private static InterviewViewModel interviewViewModel;
        private static QuestionnaireDocument questionnarie;
        private static QuestionnaireRosterStructure rosterStructure;
        private static InterviewSynchronizationDto interviewSynchronizationDto;

        private static Guid prefilledNumeric = Guid.Parse("33333333333333333333333333333333");
        private static Guid firstLevelRosterId = Guid.Parse("10000000000000000000000000000000");
        private static Guid questionInRosterReferencePrefilledNumericId = Guid.Parse("20000000000000000000000000000000");
    }
}