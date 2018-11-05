using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using WB.Services.Export.CsvExport.Exporters;
using WB.Services.Export.Interview;
using WB.Services.Export.Interview.Entities;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Tests.Questionnaire.ExportStructureFactoryTests;

namespace WB.Services.Export.Tests.CsvExport.Exporters
{
    internal class when_creating_interview_export_view_by_interview_with_roster_triggered_by_text_list_question : ExportViewFactoryTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionInsideRosterGroupId = Guid.Parse("12222222222222222222222222222222");
            rosterId = Guid.Parse("13333333333333333333333333333333");

            levelCount = 2;

            rosterSizeQuestionId = Guid.Parse("10000000000000000000000000000000");

            questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(
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
                    Children = new List<IQuestionnaireEntity>
                    {
                        new NumericQuestion()
                        {
                            PublicKey = questionInsideRosterGroupId,
                            QuestionType = QuestionType.Numeric,
                            VariableName = "q1"
                        }
                    }
                });


            exporter = Create.InterviewsExporter();

            BecauseOf();
        }

        public void BecauseOf() =>
             result = exporter.CreateInterviewDataExportView(Create.QuestionnaireExportStructure(questionnaireDocument),
                CreateInterviewDataWith2PropagatedLevels(), questionnaireDocument);

        [NUnit.Framework.Test] public void should_records_count_equals_4 () =>
           GetLevel(result, new[] { rosterSizeQuestionId }).Records.Length.Should().Be(2);

        [NUnit.Framework.Test] public void should_first_record_id_equals_0 () =>
           GetLevel(result, new[] { rosterSizeQuestionId }).Records[0].RecordId.Should().Be("0");

        [NUnit.Framework.Test] public void should_first_record_has_one_question () =>
          GetLevel(result, new[] { rosterSizeQuestionId }).Records[0].GetPlainAnswers().Count().Should().Be(1);

        [NUnit.Framework.Test] public void should_first_record_has_question_with_one_answer () =>
          GetLevel(result, new[] { rosterSizeQuestionId }).Records[0].GetPlainAnswers().First().Length.Should().Be(1);

        [NUnit.Framework.Test] public void should_first_record_has_question_with_answer_equal_to_some_answer () =>
         GetLevel(result, new[] { rosterSizeQuestionId }).Records[0].GetPlainAnswers().First().First().Should().Be(someAnswer);

        [NUnit.Framework.Test] public void should_second_record_id_equals_1 () =>
           GetLevel(result, new[] { rosterSizeQuestionId }).Records[1].RecordId.Should().Be("1");

        [NUnit.Framework.Test] public void should_second_record_has_one_question () =>
          GetLevel(result, new[] { rosterSizeQuestionId }).Records[1].GetPlainAnswers().Count().Should().Be(1);

        [NUnit.Framework.Test] public void should_second_record_has_question_with_one_answer () =>
          GetLevel(result, new[] { rosterSizeQuestionId }).Records[1].GetPlainAnswers().First().Length.Should().Be(1);

        [NUnit.Framework.Test] public void should_second_record_has_question_with_answer_equal_to_some_answer () =>
         GetLevel(result, new[] { rosterSizeQuestionId }).Records[1].GetPlainAnswers().First().First().Should().Be(someAnswer);

        [NUnit.Framework.Test] public void should_have_one_question_on_top_level () =>
            GetLevel(result, new Guid[0]).Records[0].GetPlainAnswers().Count().Should().Be(1);

        [NUnit.Framework.Test] public void should_have_five_columns_for_question_on_top_level () =>
            GetLevel(result, new Guid[0]).Records[0].GetPlainAnswers().First().Length.Should().Be(5);

        [NUnit.Framework.Test] public void should_have_first_column_with_value_a1_for_question_on_top_level () =>
            GetLevel(result, new Guid[0]).Records[0].GetPlainAnswers().First().First().Should().Be("a1");

        [NUnit.Framework.Test] public void should_have_second_column_with_value_a1_for_question_on_top_level () =>
            GetLevel(result, new Guid[0]).Records[0].GetPlainAnswers().First().Skip(1).First().Should().Be("a2");

        [NUnit.Framework.Test] public void should_have_all_other_columns_with_missing_values_for_question_on_top_level () =>
           GetLevel(result, new Guid[0]).Records[0].GetPlainAnswers().First().Skip(2).Any(a => a != ExportFormatSettings.MissingStringQuestionValue).Should().BeFalse();

        private static InterviewData CreateInterviewDataWith2PropagatedLevels()
        {
            InterviewData interview = CreateInterviewData();
            if (!interview.Levels["#"].QuestionsSearchCache.ContainsKey(rosterSizeQuestionId))
                interview.Levels["#"].QuestionsSearchCache.Add(rosterSizeQuestionId, 
                    Create.InterviewEntity(identity: Create.Identity(rosterSizeQuestionId), 
                        asList: new[]{new InterviewTextListAnswer(1, "a1"), new InterviewTextListAnswer(2, "a2")}));

            for (int i = 0; i < levelCount; i++)
            {
                var vector = new [] { i };
                var newLevel = new InterviewLevel(new ValueVector<Guid> { rosterSizeQuestionId }, null, vector);
                interview.Levels.Add(string.Join(",", vector), newLevel);

                if (!newLevel.QuestionsSearchCache.ContainsKey(questionInsideRosterGroupId))
                    newLevel.QuestionsSearchCache.Add(questionInsideRosterGroupId, 
                        Create.InterviewEntity(identity: Create.Identity(questionInsideRosterGroupId), asList: new []{new InterviewTextListAnswer(1, someAnswer)})
                        );

            }

            return interview;
        }

        private static InterviewDataExportView result;
        private static Guid rosterId;
        private static Guid rosterSizeQuestionId;
        private static Guid questionInsideRosterGroupId;
        private static int levelCount;
        private static QuestionnaireDocument questionnaireDocument;
        private static string someAnswer = "some answer";
        private static int maxAnswerCount = 5;
        private static QuestionnaireExportStructureFactory QuestionnaireExportStructureFactory;
        private InterviewsExporter exporter;
    }
}
