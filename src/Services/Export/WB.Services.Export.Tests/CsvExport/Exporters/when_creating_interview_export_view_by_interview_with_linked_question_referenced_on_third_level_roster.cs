using System;
using System.Linq;
using FluentAssertions;
using WB.Services.Export.CsvExport.Exporters;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Tests.Questionnaire.ExportStructureFactoryTests;

namespace WB.Services.Export.Tests.CsvExport.Exporters
{
    internal class when_creating_interview_export_view_by_interview_with_linked_question_referenced_on_third_level_roster : ExportViewFactoryTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            linkedQuestionSourceId = Guid.Parse("12222222222222222222222222222222");
            rosterId = Guid.Parse("13333333333333333333333333333333");
            var nestedRosterId = Guid.Parse("23333333333333333333333333333333");

            linkedQuestionId = Guid.Parse("10000000000000000000000000000000");

            questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(
                new SingleQuestion
                {
                    PublicKey = linkedQuestionId,
                    QuestionType = QuestionType.SingleOption,
                    LinkedToQuestionId = linkedQuestionSourceId
                },
                Create.Roster(rosterId: rosterId, obsoleteFixedTitles: new[] {"t1", "t2"},
                    children: new IQuestionnaireEntity[]
                    {
                        Create.Roster(rosterId: nestedRosterId, obsoleteFixedTitles: new[] {"n1", "n2"},
                            children: new IQuestionnaireEntity[]
                            {
                                Create.NumericRealQuestion(id: linkedQuestionSourceId, variable: "q1")
                            })
                    }));

            interview = CreateInterviewData();

            if (!interview.Levels["#"].QuestionsSearchCache.ContainsKey(linkedQuestionId))
                interview.Levels["#"].QuestionsSearchCache.Add(linkedQuestionId, Create.InterviewEntity(identity: Create.Identity(linkedQuestionId), asIntArray:new int[] { 1 , 4 }));

            exporter = Create.InterviewsExporter();

            BecauseOf();
        }

        private void BecauseOf() 
        {
            result =
                exporter.CreateInterviewDataExportView(
                    Create.QuestionnaireExportStructure(questionnaireDocument), interview, questionnaireDocument);
        }

        [NUnit.Framework.Test] public void should_linked_question_have_one_answer () =>
           GetLevel(result, new Guid[0]).Records[0].GetPlainAnswers().First().Length.Should().Be(1);

        [NUnit.Framework.Test] public void should_linked_question_have_first_answer_be_equal_to_0 () =>
           GetLevel(result, new Guid[0]).Records[0].GetPlainAnswers().First().First().Should().Be("[1|4]");

        private static InterviewDataExportView result;
        private static Guid rosterId;
        private static Guid linkedQuestionId;
        private static Guid linkedQuestionSourceId;
        private static QuestionnaireDocument questionnaireDocument;
        private static InterviewData interview;
        private InterviewsExporter exporter;
    }
}
