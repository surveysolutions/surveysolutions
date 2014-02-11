﻿using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Supervisor.Views.DataExport;
using WB.Core.BoundedContexts.Supervisor.Views.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Interview;

namespace WB.Core.BoundedContexts.Supervisor.Tests.EventHandlers.InterviewExportedDataEventHandlerTests
{
    internal class when_InterviewApproved_recived_by_interview_with_roster_triggered_by_text_list_question : InterviewExportedDataEventHandlerTestContext
    {
        Establish context = () =>
        {
            questionInsideRosterGroupId = Guid.Parse("12222222222222222222222222222222");
            rosterId = Guid.Parse("13333333333333333333333333333333");

            levelCount = 2;

            rosterSizeQuestionId = Guid.Parse("10000000000000000000000000000000");

            questionnarie = CreateQuestionnaireDocumentWithOneChapter(
                new TextListQuestion()
                {
                    PublicKey = rosterSizeQuestionId,
                    QuestionType = QuestionType.TextList,
                    MaxAnswerCount = maxAnswerCount
                },
                new Group()
                {
                    PublicKey = rosterId,
                    IsRoster = true,
                    RosterSizeQuestionId = rosterSizeQuestionId,
                    Children = new List<IComposite>
                    {
                        new NumericQuestion()
                        {
                            PublicKey = questionInsideRosterGroupId,
                            QuestionType = QuestionType.Numeric,
                            StataExportCaption = "q1"
                        }
                    }
                });

            interviewExportedDataEventHandler = CreateInterviewExportedDataEventHandlerForQuestionnarieCreatedByMethod(
                () => questionnarie,
                CreateInterviewDataWith2PropagatedLevels, r => result = r);
        };

        Because of = () =>
             interviewExportedDataEventHandler.Handle(CreatePublishableEvent());

        It should_records_count_equals_4 = () =>
           GetLevel(result, rosterSizeQuestionId).Records.Length.ShouldEqual(2);

        It should_first_record_id_equals_0 = () =>
           GetLevel(result, rosterSizeQuestionId).Records[0].RecordId.ShouldEqual(0);

        It should_first_record_has_one_question = () =>
          GetLevel(result, rosterSizeQuestionId).Records[0].Questions.Length.ShouldEqual(1);

        It should_first_record_has_question_with_id_of_questionInsideRosterGroupId = () =>
          GetLevel(result, rosterSizeQuestionId).Records[0].Questions[0].QuestionId.ShouldEqual(questionInsideRosterGroupId);

        It should_first_record_has_question_with_one_answer = () =>
          GetLevel(result, rosterSizeQuestionId).Records[0].Questions[0].Answers.Length.ShouldEqual(1);

        It should_first_record_has_question_with_answer_equal_to_some_answer = () =>
         GetLevel(result, rosterSizeQuestionId).Records[0].Questions[0].Answers[0].ShouldEqual(someAnswer);

        It should_second_record_id_equals_1 = () =>
           GetLevel(result, rosterSizeQuestionId).Records[1].RecordId.ShouldEqual(1);

        It should_second_record_has_one_question = () =>
          GetLevel(result, rosterSizeQuestionId).Records[1].Questions.Length.ShouldEqual(1);

        It should_second_record_has_question_with_id_of_questionInsideRosterGroupId = () =>
          GetLevel(result, rosterSizeQuestionId).Records[1].Questions[0].QuestionId.ShouldEqual(questionInsideRosterGroupId);

        It should_second_record_has_question_with_one_answer = () =>
          GetLevel(result, rosterSizeQuestionId).Records[1].Questions[0].Answers.Length.ShouldEqual(1);

        It should_second_record_has_question_with_answer_equal_to_some_answer = () =>
         GetLevel(result, rosterSizeQuestionId).Records[1].Questions[0].Answers[0].ShouldEqual(someAnswer);

        It should_have_one_question_on_top_level = () =>
            GetLevel(result, questionnarie.PublicKey).Records[0].Questions.Length.ShouldEqual(1);

        It should_have_five_columns_for_question_on_top_level = () =>
            GetLevel(result, questionnarie.PublicKey).Records[0].Questions[0].Answers.Length.ShouldEqual(5);

        It should_have_first_column_with_value_a1_for_question_on_top_level = () =>
            GetLevel(result, questionnarie.PublicKey).Records[0].Questions[0].Answers[0].ShouldEqual("a1");

        It should_have_second_column_with_value_a1_for_question_on_top_level = () =>
            GetLevel(result, questionnarie.PublicKey).Records[0].Questions[0].Answers[1].ShouldEqual("a2");

        It should_have_all_other_coulmns_with_empty_values_for_question_on_top_level = () =>
           GetLevel(result, questionnarie.PublicKey).Records[0].Questions[0].Answers.Skip(2).Any(a=>!string.IsNullOrEmpty(a)).ShouldBeFalse();

        private static InterviewDataExportLevelView GetLevel(InterviewDataExportView interviewDataExportView, Guid levelId)
        {
            return interviewDataExportView.Levels.FirstOrDefault(l => l.LevelId == levelId);
        }

        private static InterviewData CreateInterviewDataWith2PropagatedLevels()
        {
            InterviewData interview = CreateInterviewData();
            
            var textListQuestion = interview.Levels["#"].GetOrCreateQuestion(rosterSizeQuestionId);
            textListQuestion.Answer = new string[] { "a1", "a2" };

            for (int i = 0; i < levelCount; i++)
            {
                var vector = new decimal[1] { i };
                var newLevel = new InterviewLevel(rosterSizeQuestionId, null, vector);
                interview.Levels.Add(string.Join(",", vector), newLevel);

                var question = newLevel.GetOrCreateQuestion(questionInsideRosterGroupId);
                question.Answer = new InterviewTextListAnswers(new[] { new Tuple<decimal, string>(1, someAnswer) });
            }

            return interview;
        }

        private static EventHandler.InterviewExportedDataEventHandler interviewExportedDataEventHandler;
        private static InterviewDataExportView result;
        private static Guid rosterId;
        private static Guid rosterSizeQuestionId;
        private static Guid questionInsideRosterGroupId;
        private static int levelCount;
        private static QuestionnaireDocument questionnarie;
        private static string someAnswer = "some answer";
        private static int maxAnswerCount = 5;
    }
}
