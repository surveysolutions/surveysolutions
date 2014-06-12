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
    internal class when_cteating_interview_with_sync_data_and_questionnaire_has_questions_that_reference_prefilled_one_in_title : InterviewViewModelTestContext
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
                new TextQuestion
                {
                    PublicKey = questionReferencePrefilledNumericId,
                    QuestionType = QuestionType.Text,
                    QuestionText = "Hello, %numeric%"
                },
                 new Group
                {
                    PublicKey = firstLevelRosterId,
                    IsRoster = true,
                    RosterSizeQuestionId = prefilledNumeric,
                    Children = new List<IComposite>()
                    {
                        new Group
                        {
                            PublicKey = secondLevelRosterId,
                            IsRoster = true,
                            RosterSizeSource = RosterSizeSourceType.FixedTitles,
                            RosterFixedTitles = new[] { "t1", "t2" },
                            Children = new List<IComposite>()
                            {
                                new NumericQuestion
                                    {
                                        PublicKey = questionInRosterReferencePrefilledNumericId,
                                        QuestionText = "Example %numeric%",
                                        QuestionType = QuestionType.Numeric
                                    }

                            }
                        },
                        new NumericQuestion() { PublicKey = sourceForLinkedQuestionId, QuestionType = QuestionType.Numeric }
                    }
                });

            rosterStructure = CreateQuestionnaireRosterStructure(questionnarie);

            interviewSynchronizationDto = CreateInterviewSynchronizationDto(
                answers: new[]
                {
                    new AnsweredQuestionSynchronizationDto(prefilledNumeric, new decimal[0], 2, string.Empty),
                },
                propagatedGroupInstanceCounts: new Dictionary<InterviewItemId, RosterSynchronizationDto[]>()
                {
                    {
                        new InterviewItemId(firstLevelRosterId, new decimal[0]),
                        new[]
                        {
                            new RosterSynchronizationDto(firstLevelRosterId, new decimal[0], 0, null, null),
                            new RosterSynchronizationDto(firstLevelRosterId, new decimal[0], 1, null, null),

                            new RosterSynchronizationDto(secondLevelRosterId, new decimal[] { 1 }, 0, null, null)
                        }
                    },
                    {
                        new InterviewItemId(secondLevelRosterId, new decimal[] { 0 }),
                        new[]
                        {
                            new RosterSynchronizationDto(secondLevelRosterId, new decimal[] { 0 }, 0, null, null),
                            new RosterSynchronizationDto(secondLevelRosterId, new decimal[] { 0 }, 1, null, null),
                        }
                    },
                    {
                        new InterviewItemId(secondLevelRosterId, new decimal[] { 1 }),
                        new[]
                        {
                            new RosterSynchronizationDto(secondLevelRosterId, new decimal[] { 1 }, 0, null, null),
                            new RosterSynchronizationDto(secondLevelRosterId, new decimal[] { 1 }, 1, null, null)
                        }
                    }
                });
        };

        Because of = () =>
            interviewViewModel = CreateInterviewViewModel(questionnarie, rosterStructure, interviewSynchronizationDto);

        It should_title_of_question_with_substitution_in_chapter_be_substituted_with_answer_on_prefilled_question = () =>
            GetQuestion(questionReferencePrefilledNumericId, new decimal[0]).Text.ShouldEqual("Hello, 2");

        It should_title_of_question_with_substitution_in_first_row_of_first_roster_be_substituted_with_answer_on_set_question = () =>
            GetQuestion(questionInRosterReferencePrefilledNumericId, new decimal[]{ 0, 0}).Text.ShouldEqual("Example 2");

        It should_title_of_question_with_substitution_in_first_row_of_second_roster_be_substituted_with_answer_on_set_question = () =>
           GetQuestion(questionInRosterReferencePrefilledNumericId, new decimal[] { 1, 0 }).Text.ShouldEqual("Example 2");

        It should_title_of_question_with_substitution_in_second_row_of_first_roster_be_substituted_with_answer_on_set_question = () =>
            GetQuestion(questionInRosterReferencePrefilledNumericId, new decimal[] { 0, 1 }).Text.ShouldEqual("Example 2");

        It should_title_of_question_with_substitution_in_second_row_of_second_roster_be_substituted_with_answer_on_set_question = () =>
           GetQuestion(questionInRosterReferencePrefilledNumericId, new decimal[] { 1, 1 }).Text.ShouldEqual("Example 2");

        private static QuestionViewModel GetQuestion(Guid questionId, decimal[] rosterVector)
        {
            return interviewViewModel.FindQuestion(q => q.PublicKey == Create.InterviewItemId(questionId, rosterVector)).FirstOrDefault();
        }

        private static InterviewViewModel interviewViewModel;
        private static QuestionnaireDocument questionnarie;
        private static QuestionnaireRosterStructure rosterStructure;
        private static InterviewSynchronizationDto interviewSynchronizationDto;

        private static Guid prefilledNumeric = Guid.Parse("33333333333333333333333333333333");
        private static Guid nestedGroupId = Guid.Parse("22222222222222222222222222222222");
        private static Guid sourceForLinkedQuestionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid firstLevelRosterId = Guid.Parse("10000000000000000000000000000000");
        private static Guid secondLevelRosterId = Guid.Parse("44444444444444444444444444444444");
        private static Guid questionInRosterReferencePrefilledNumericId = Guid.Parse("20000000000000000000000000000000");
        private static Guid questionReferencePrefilledNumericId = Guid.Parse("30000000000000000000000000000000");
    }
}