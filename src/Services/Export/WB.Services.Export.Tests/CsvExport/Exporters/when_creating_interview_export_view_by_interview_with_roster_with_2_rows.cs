using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using WB.Services.Export.CsvExport.Exporters;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.Tests.CsvExport.Exporters
{
    class when_creating_interview_export_view_by_interview_with_roster_with_2_rows : ExportViewFactoryTests
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            firstQuestionId = Guid.Parse("12222222222222222222222222222222");
            secondQuestionId = Guid.Parse("11111111111111111111111111111111");
            propagatedGroup = Guid.Parse("13333333333333333333333333333333");

            levelCount = 2;

            variableNameAndQuestionId = new Dictionary<string, Guid>
            {
                {"q1", firstQuestionId},
                {"q2", secondQuestionId}
            };

            propagationScopeKey = Guid.Parse("10000000000000000000000000000000");
            questionnaireDocument = CreateQuestionnaireDocumentWith1PropagationLevel();

            exporter = Create.InterviewsExporter();

            BecauseOf();
        }

        private void BecauseOf() =>
            result = exporter.CreateInterviewDataExportView(Create.QuestionnaireExportStructure(questionnaireDocument),
                CreateInterviewDataWith2PropagatedLevels(), questionnaireDocument);

        [NUnit.Framework.Test] public void should_records_count_equals_4 () =>
           GetLevel(result, new[] { propagationScopeKey }).Records.Length.Should().Be(2);

        [NUnit.Framework.Test] public void should_first_record_id_equals_0 () =>
           GetLevel(result, new[] { propagationScopeKey }).Records[0].RecordId.Should().Be("0");

        [NUnit.Framework.Test] public void should_second_record_id_equals_1 () =>
           GetLevel(result, new[] { propagationScopeKey }).Records[1].RecordId.Should().Be("1");

        [NUnit.Framework.Test] public void should_first_rosters_record_parent_ids_contains_only_main_level_record_id () =>
          GetLevel(result, new[] { propagationScopeKey }).Records[0].ParentRecordIds.Should().BeEquivalentTo(new string[] { GetLevel(result, new Guid[0]).Records[0].RecordId });

        [NUnit.Framework.Test] public void should_second_rosters_record_parent_ids_contains_only_main_level_record_id () =>
           GetLevel(result, new[] { propagationScopeKey }).Records[1].ParentRecordIds.Should().BeEquivalentTo(new string[] { GetLevel(result, new Guid[0]).Records[0].RecordId});

        private static QuestionnaireDocument CreateQuestionnaireDocumentWith1PropagationLevel()
        {
            return Create.QuestionnaireDocumentWithOneChapter(
                new NumericQuestion() { VariableName= "auto", PublicKey = propagationScopeKey, QuestionType = QuestionType.Numeric },
                new Group()
                {
                    PublicKey = propagatedGroup, IsRoster = true, RosterSizeQuestionId = propagationScopeKey, 
                    Children = variableNameAndQuestionId.Select(x => new NumericQuestion() {VariableName = x.Key, PublicKey = x.Value })
                        
                }
            );
        }

        private static InterviewData CreateInterviewDataWith2PropagatedLevels()
        {
            InterviewData interview = CreateInterviewData();
            for (int i = 0; i < levelCount; i++)
            {
                var vector = new int[1] { i };
                var newLevel = new InterviewLevel(new ValueVector<Guid> { propagationScopeKey }, null, vector);
                interview.Levels.Add(string.Join(",", vector), newLevel);

                foreach (var questionId in variableNameAndQuestionId)
                {
                    if (!newLevel.QuestionsSearchCache.ContainsKey(questionId.Value))
                        newLevel.QuestionsSearchCache.Add(questionId.Value, Create.InterviewEntity(identity: Create.Identity(questionId.Value), asString:"some answer"));
                }
            }
            return interview;
        }

        private static InterviewDataExportView result;
        private static Dictionary<string, Guid> variableNameAndQuestionId;
        private static Guid propagatedGroup;
        private static Guid propagationScopeKey;
        private static Guid secondQuestionId;
        private static Guid firstQuestionId;
        private static int levelCount;
        private static QuestionnaireDocument questionnaireDocument;
        private static QuestionnaireExportStructureFactory QuestionnaireExportStructureFactory;
        private InterviewsExporter exporter;
    }
}
