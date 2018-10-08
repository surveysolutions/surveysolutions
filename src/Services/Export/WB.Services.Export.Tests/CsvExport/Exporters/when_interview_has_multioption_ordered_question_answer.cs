using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using WB.Services.Export.CsvExport.Exporters;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.Tests.CsvExport.Exporters
{
    internal class when_interview_has_multioption_ordered_question_answer : ExportViewFactoryTests
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionId = Guid.Parse("d7127d06-5668-4fa3-b255-8a2a0aaaa020");

            questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(
                Create.MultyOptionsQuestion(id: questionId, areAnswersOrdered:true, options: new List<Answer> { Create.Answer("foo", 28), Create.Answer("bar", 42) }));

            interview = CreateInterviewData(Create.InterviewEntity(identity: Create.Identity(questionId), asIntArray: new[] { 42, 28 }));
            exporter = Create.InterviewsExporter();

            BecauseOf();
        }

        public void BecauseOf() => result = exporter.CreateInterviewDataExportView(Create.QuestionnaireExportStructure(questionnaireDocument), interview, questionnaireDocument);

        [NUnit.Framework.Test] public void should_put_answers_to_export () => result.Levels.Length.Should().Be(1);

        [NUnit.Framework.Test] public void should_put_answers_to_export_in_appropriate_order () 
        {
            InterviewDataExportLevelView first = result.Levels.First();
            var exportedQuestion = first.Records.First().GetPlainAnswers().First();
            exportedQuestion.Length.Should().Be(2);
            exportedQuestion.Should().BeEquivalentTo(new[] { "2", "1" });
        }

        static QuestionnaireExportStructureFactory QuestionnaireExportStructureFactory;
        static QuestionnaireExportStructure questionnaaireExportStructure;
        static InterviewDataExportView result;
        static InterviewData interview;
        static Guid questionId;
        private QuestionnaireDocument questionnaireDocument;
        private InterviewsExporter exporter;
    }
}
