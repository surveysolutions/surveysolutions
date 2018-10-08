using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using WB.Services.Export.CsvExport.Exporters;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Questionnaire.Services;
using WB.Services.Export.Tests.Questionnaire.ExportStructureFactoryTests;

namespace WB.Services.Export.Tests.CsvExport.Exporters
{
    internal class when_creating_interview_export_view_by_interview_with_roster_with_2_rows_with_nested_groups : ExportViewFactoryTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionInsideNestedGroupId = Guid.Parse("12222222222222222222222222222222");
            rosterId = Guid.Parse("13333333333333333333333333333333");

            levelCount = 2;

            rosterSizeQuestionId = Guid.Parse("10000000000000000000000000000000");

            var nestedGroupId = Guid.Parse("11111111111111111111111111111111");

            questionnaireDocument = CreateQuestionnaireDocumentWithOneChapter(
                Create.NumericRealQuestion(id: rosterSizeQuestionId),
                Create.Roster(rosterId: rosterId, rosterSizeQuestionId: rosterSizeQuestionId,
                    children: new List<IQuestionnaireEntity>
                    {
                        Create.Group(nestedGroupId, Create.NumericIntegerQuestion(id: questionInsideNestedGroupId, variable: "q1"))
                    }));

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

        [NUnit.Framework.Test] public void should_first_record_has_question_with_oneanswer () =>
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

        private static InterviewData CreateInterviewDataWith2PropagatedLevels()
        {
            InterviewData interview = CreateInterviewData();
            for (int i = 0; i < levelCount; i++)
            {
                var vector = new int[1] { i };
                var newLevel = new InterviewLevel(new ValueVector<Guid> { rosterSizeQuestionId }, null, vector);
                interview.Levels.Add(string.Join(",", vector), newLevel);
                
                if (!newLevel.QuestionsSearchCache.ContainsKey(questionInsideNestedGroupId))
                    newLevel.QuestionsSearchCache.Add(questionInsideNestedGroupId, Create.InterviewEntity(identity: Create.Identity(questionInsideNestedGroupId),
                        asString: someAnswer));
            }

            return interview;
        }

        private static InterviewDataExportView result;
        private static Guid rosterId;
        private static Guid rosterSizeQuestionId;
        private static Guid questionInsideNestedGroupId;
        private static int levelCount;
        private static QuestionnaireDocument questionnaireDocument;
        private static string someAnswer = "some answer";
        private static QuestionnaireExportStructureFactory QuestionnaireExportStructureFactory;
        private InterviewsExporter exporterS;
        private InterviewsExporter exporter;
    }
}
