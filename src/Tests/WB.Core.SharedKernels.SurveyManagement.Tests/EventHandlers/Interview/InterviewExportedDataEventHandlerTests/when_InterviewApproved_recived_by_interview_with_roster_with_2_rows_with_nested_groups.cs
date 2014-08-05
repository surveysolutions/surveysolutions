using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.EventHandlers.Interview.InterviewExportedDataEventHandlerTests
{
    internal class when_InterviewApproved_recived_by_interview_with_roster_with_2_rows_with_nested_groups : InterviewExportedDataEventHandlerTestContext
    {
        Establish context = () =>
        {
            questionInsideNestedGroupId = Guid.Parse("12222222222222222222222222222222");
            rosterId = Guid.Parse("13333333333333333333333333333333");

            levelCount = 2;

            rosterSizeQuestionId = Guid.Parse("10000000000000000000000000000000");

            var nestedGroupId = Guid.Parse("11111111111111111111111111111111");

            questionnarie = CreateQuestionnaireDocumentWithOneChapter(
                new NumericQuestion()
                {
                    PublicKey = rosterSizeQuestionId,
                    QuestionType = QuestionType.Numeric
                },
                new Group()
                {
                    PublicKey = rosterId,
                    IsRoster = true,
                    RosterSizeQuestionId = rosterSizeQuestionId,
                    Children = new List<IComposite>
                    {
                        new Group("title")
                        {
                            PublicKey = nestedGroupId,
                            Children = new List<IComposite>
                            {
                                new NumericQuestion()
                                {
                                    PublicKey = questionInsideNestedGroupId,
                                    QuestionType = QuestionType.Numeric,
                                    StataExportCaption = "q1"
                                }
                            }
                        }
                    }
                }); 

            interviewExportedDataDenormalizer = CreateInterviewExportedDataEventHandlerForQuestionnarieCreatedByMethod(
                () => questionnarie,
                CreateInterviewDataWith2PropagatedLevels, r => result = r);
        };

        Because of = () =>
            interviewExportedDataDenormalizer.Handle(CreateInterviewApprovedByHQPublishableEvent());

        It should_records_count_equals_4 = () =>
           GetLevel(result, new[] { rosterSizeQuestionId }).Records.Length.ShouldEqual(2);

        It should_first_record_id_equals_0 = () =>
           GetLevel(result, new[] { rosterSizeQuestionId }).Records[0].RecordId.ShouldEqual("0");

        It should_first_record_has_one_question = () =>
          GetLevel(result, new[] { rosterSizeQuestionId }).Records[0].Questions.Length.ShouldEqual(1);

        It should_first_record_has_question_with_id_of_questionInsideNestedGroupId = () =>
          GetLevel(result, new[] { rosterSizeQuestionId }).Records[0].Questions[0].QuestionId.ShouldEqual(questionInsideNestedGroupId);

        It should_first_record_has_question_with_oneanswer = () =>
          GetLevel(result, new[] { rosterSizeQuestionId }).Records[0].Questions[0].Answers.Length.ShouldEqual(1);

        It should_first_record_has_question_with_answer_equal_to_some_answer = () =>
         GetLevel(result, new[] { rosterSizeQuestionId }).Records[0].Questions[0].Answers[0].ShouldEqual(someAnswer);

        It should_second_record_id_equals_1 = () =>
           GetLevel(result, new[] { rosterSizeQuestionId }).Records[1].RecordId.ShouldEqual("1");

        It should_second_record_has_one_question = () =>
          GetLevel(result, new[] { rosterSizeQuestionId }).Records[1].Questions.Length.ShouldEqual(1);

        It should_second_record_has_question_with_id_of_questionInsideNestedGroupId = () =>
          GetLevel(result, new[] { rosterSizeQuestionId }).Records[1].Questions[0].QuestionId.ShouldEqual(questionInsideNestedGroupId);

        It should_second_record_has_question_with_one_answer = () =>
          GetLevel(result, new[] { rosterSizeQuestionId }).Records[1].Questions[0].Answers.Length.ShouldEqual(1);

        It should_second_record_has_question_with_answer_equal_to_some_answer = () =>
         GetLevel(result, new[] { rosterSizeQuestionId }).Records[1].Questions[0].Answers[0].ShouldEqual(someAnswer);

        private static InterviewData CreateInterviewDataWith2PropagatedLevels()
        {
            InterviewData interview = CreateInterviewData();
            for (int i = 0; i < levelCount; i++)
            {
                var vector = new decimal[1] { i };
                var newLevel = new InterviewLevel(new ValueVector<Guid> { rosterSizeQuestionId }, null, vector);
                interview.Levels.Add(string.Join(",", vector), newLevel);

                var question = newLevel.GetOrCreateQuestion(questionInsideNestedGroupId);
                question.Answer = someAnswer;
            }

            return interview;
        }

        private static InterviewExportedDataDenormalizer interviewExportedDataDenormalizer;
        private static InterviewDataExportView result;
        private static Guid rosterId;
        private static Guid rosterSizeQuestionId;
        private static Guid questionInsideNestedGroupId;
        private static int levelCount;
        private static QuestionnaireDocument questionnarie;
        private static string someAnswer = "some answer";
    }
}
