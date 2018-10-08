using System;
using System.Linq;
using FluentAssertions;
using WB.Services.Export.CsvExport.Exporters;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.Tests.CsvExport.Exporters
{
    internal class when_interview_has_multioption_linked_question_answer : ExportViewFactoryTests
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            multyOptionLinkedQuestionId = Guid.Parse("d7127d06-5668-4fa3-b255-8a2a0aaaa020");
            linkedSourceQuestionId = Guid.NewGuid();

            questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(
                Create.Roster(rosterId: Guid.NewGuid(), 
                              variable: "row", 
                              obsoleteFixedTitles: new[] { "1", "2" },
                              children: new[] {
                                  Create.TextQuestion(id: linkedSourceQuestionId, variable: "varTxt")
                              }),
                Create.MultyOptionsQuestion(id: multyOptionLinkedQuestionId,
                    variable: "mult",
                    linkedToQuestionId: linkedSourceQuestionId));

            exportStructure = Create.QuestionnaireExportStructure(questionnaireDocument);
            exporter = Create.InterviewsExporter();

            interview = CreateInterviewData(Create.InterviewEntity(identity: Create.Identity(multyOptionLinkedQuestionId), asIntArray:new[] { 2 }));
            BecauseOf();
        }

        public void BecauseOf() => result = exporter.CreateInterviewDataExportView(exportStructure, interview, questionnaireDocument);

        [NUnit.Framework.Test] public void should_put_answers_to_export_in_appropriate_order () 
        {
            InterviewDataExportLevelView first = result.Levels.First();
            var exportedQuestion = first.Records.First().GetPlainAnswers().First();
            exportedQuestion.Length.Should().Be(2);
            exportedQuestion.Should().BeEquivalentTo(new[] {"2", ExportFormatSettings.MissingStringQuestionValue});
        }

        static QuestionnaireExportStructure exportStructure;
        static InterviewDataExportView result;
        static InterviewData interview;
        static Guid multyOptionLinkedQuestionId;
        static Guid linkedSourceQuestionId;
        private InterviewsExporter exporter;
        private QuestionnaireDocument questionnaireDocument;
    }
}
