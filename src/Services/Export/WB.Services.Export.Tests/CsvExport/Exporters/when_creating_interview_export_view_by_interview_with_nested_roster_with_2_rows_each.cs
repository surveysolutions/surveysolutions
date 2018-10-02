using System;
using FluentAssertions;
using WB.Services.Export.CsvExport.Exporters;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.Tests.CsvExport.Exporters
{
    internal class when_creating_interview_export_view_by_interview_with_nested_roster_with_2_rows_each : ExportViewFactoryTests
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionInsideRosterGroupId = Guid.Parse("12222222222222222222222222222222");
            rosterId = Guid.Parse("11111111111111111111111111111111");
            nestedRosterId = Guid.Parse("13333333333333333333333333333333");

            questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(
                Create.Roster(rosterId: rosterId, fixedTitles: new FixedRosterTitle[] {new FixedRosterTitle(1, "t1"), new FixedRosterTitle(2, "t2")},
                    children: new IQuestionnaireEntity[]
                    {
                        Create.Roster(rosterId: nestedRosterId, fixedTitles: new FixedRosterTitle[] {new FixedRosterTitle(1, "t1"), new FixedRosterTitle(2, "t2")},
                            children: new IQuestionnaireEntity[]
                            {
                                Create.NumericRealQuestion(id: questionInsideRosterGroupId, variable: "q1")
                            })
                    }));
            exporter = Create.InterviewsExporter();
            BecauseOf();
        }

        private void BecauseOf() =>
               result = exporter.CreateInterviewDataExportView(Create.QuestionnaireExportStructure(questionnaireDocument),
                CreateInterviewDataWith2PropagatedLevels(), questionnaireDocument);

        [NUnit.Framework.Test] public void should_records_count_equals_4 () =>
           GetLevel(result, new[] { rosterId, nestedRosterId }).Records.Length.Should().Be(4);

        [NUnit.Framework.Test] public void should_first_record_id_equals_0 () =>
           GetLevel(result, new[] { rosterId, nestedRosterId }).Records[0].RecordId.Should().Be("0");

        [NUnit.Framework.Test] public void should_second_record_id_equals_1 () =>
           GetLevel(result, new[] { rosterId, nestedRosterId }).Records[1].RecordId.Should().Be("1");

        [NUnit.Framework.Test] public void should_third_record_id_equals_1 () =>
           GetLevel(result, new[] { rosterId, nestedRosterId }).Records[2].RecordId.Should().Be("0");

        [NUnit.Framework.Test] public void should_fourth_record_id_equals_1 () =>
           GetLevel(result, new[] { rosterId, nestedRosterId }).Records[3].RecordId.Should().Be("1");

        private static InterviewData CreateInterviewDataWith2PropagatedLevels()
        {
            InterviewData interview = CreateInterviewData();
            for (int i = 0; i < levelCount; i++)
            {
                var vector = new int[1] { i };
                var newLevel = new InterviewLevel(new ValueVector<Guid> { rosterId }, null, vector);
                interview.Levels.Add(string.Join(",", vector), newLevel);
                for (int j = 0; j < levelCount; j++)
                {
                    var nestedVector = new int[] { i, j };
                    var nestedLevel = new InterviewLevel(new ValueVector<Guid> { rosterId, nestedRosterId }, null, nestedVector);
                    interview.Levels.Add(string.Join(",", nestedVector), nestedLevel);

                    if (!nestedLevel.QuestionsSearchCache.ContainsKey(questionInsideRosterGroupId))
                        nestedLevel.QuestionsSearchCache.Add(questionInsideRosterGroupId, Create.InterviewEntity(identity: Create.Identity(questionInsideRosterGroupId), asString: "some answer"));
                }

            }
            return interview;
        }

        private static InterviewDataExportView result;
        private static Guid nestedRosterId;
        private static Guid rosterId;
        private static Guid questionInsideRosterGroupId;
        private static int levelCount=2;
        private static QuestionnaireDocument questionnaireDocument;
        private static QuestionnaireExportStructureFactory QuestionnaireExportStructureFactory;
        private InterviewsExporter exporter;
    }
}
