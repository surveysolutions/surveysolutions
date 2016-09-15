using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Denormalizers;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.Views.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Factories.ExportViewFactoryTests
{
    internal class when_creating_interview_export_view_by_interview_with_roster_triggered_by_text_list_question : ExportViewFactoryTestsContext
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

            exportViewFactory = CreateExportViewFactory();
        };

        Because of = () =>
             result = exportViewFactory.CreateInterviewDataExportView(exportViewFactory.CreateQuestionnaireExportStructure(questionnarie, 1),
                CreateInterviewDataWith2PropagatedLevels());

        It should_records_count_equals_4 = () =>
           GetLevel(result, new[] { rosterSizeQuestionId }).Records.Length.ShouldEqual(2);

        It should_first_record_id_equals_0 = () =>
           GetLevel(result, new[] { rosterSizeQuestionId }).Records[0].RecordId.ShouldEqual("0");

        It should_first_record_has_one_question = () =>
          GetLevel(result, new[] { rosterSizeQuestionId }).Records[0].GetPlainAnswers().Count().ShouldEqual(1);

        It should_first_record_has_question_with_one_answer = () =>
          GetLevel(result, new[] { rosterSizeQuestionId }).Records[0].GetPlainAnswers().First().Length.ShouldEqual(1);

        It should_first_record_has_question_with_answer_equal_to_some_answer = () =>
         GetLevel(result, new[] { rosterSizeQuestionId }).Records[0].GetPlainAnswers().First().First().ShouldEqual(someAnswer);

        It should_second_record_id_equals_1 = () =>
           GetLevel(result, new[] { rosterSizeQuestionId }).Records[1].RecordId.ShouldEqual("1");

        It should_second_record_has_one_question = () =>
          GetLevel(result, new[] { rosterSizeQuestionId }).Records[1].GetPlainAnswers().Count().ShouldEqual(1);

        It should_second_record_has_question_with_one_answer = () =>
          GetLevel(result, new[] { rosterSizeQuestionId }).Records[1].GetPlainAnswers().First().Length.ShouldEqual(1);

        It should_second_record_has_question_with_answer_equal_to_some_answer = () =>
         GetLevel(result, new[] { rosterSizeQuestionId }).Records[1].GetPlainAnswers().First().First().ShouldEqual(someAnswer);

        It should_have_one_question_on_top_level = () =>
            GetLevel(result, new Guid[0]).Records[0].GetPlainAnswers().Count().ShouldEqual(1);

        It should_have_five_columns_for_question_on_top_level = () =>
            GetLevel(result, new Guid[0]).Records[0].GetPlainAnswers().First().Length.ShouldEqual(5);

        It should_have_first_column_with_value_a1_for_question_on_top_level = () =>
            GetLevel(result, new Guid[0]).Records[0].GetPlainAnswers().First().First().ShouldEqual("a1");

        It should_have_second_column_with_value_a1_for_question_on_top_level = () =>
            GetLevel(result, new Guid[0]).Records[0].GetPlainAnswers().First().Second().ShouldEqual("a2");

        It should_have_all_other_coulmns_with_missing_values_for_question_on_top_level = () =>
           GetLevel(result, new Guid[0]).Records[0].GetPlainAnswers().First().Skip(2).Any(a => a != ExportFormatSettings.MissingStringQuestionValue).ShouldBeFalse();

        private static InterviewData CreateInterviewDataWith2PropagatedLevels()
        {
            InterviewData interview = CreateInterviewData();
            if (!interview.Levels["#"].QuestionsSearchCache.ContainsKey(rosterSizeQuestionId))
                interview.Levels["#"].QuestionsSearchCache.Add(rosterSizeQuestionId, new InterviewQuestion(rosterSizeQuestionId));

            var textListQuestion = interview.Levels["#"].QuestionsSearchCache[rosterSizeQuestionId];
            textListQuestion.Answer = new string[] { "a1", "a2" };

            for (int i = 0; i < levelCount; i++)
            {
                var vector = new decimal[1] { i };
                var newLevel = new InterviewLevel(new ValueVector<Guid> { rosterSizeQuestionId }, null, vector);
                interview.Levels.Add(string.Join(",", vector), newLevel);

                if (!newLevel.QuestionsSearchCache.ContainsKey(questionInsideRosterGroupId))
                    newLevel.QuestionsSearchCache.Add(questionInsideRosterGroupId, new InterviewQuestion(questionInsideRosterGroupId));

                var question = newLevel.QuestionsSearchCache[questionInsideRosterGroupId];

                question.Answer = new InterviewTextListAnswers(new[] { new Tuple<decimal, string>(1, someAnswer) });
            }

            return interview;
        }

        private static InterviewDataExportView result;
        private static Guid rosterId;
        private static Guid rosterSizeQuestionId;
        private static Guid questionInsideRosterGroupId;
        private static int levelCount;
        private static QuestionnaireDocument questionnarie;
        private static string someAnswer = "some answer";
        private static int maxAnswerCount = 5;
        private static ExportViewFactory exportViewFactory;
    }
}
