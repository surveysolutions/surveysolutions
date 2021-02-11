using System.Threading;
using ApprovalTests;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using ApprovalTests.Reporters.TestFrameworks;
using NUnit.Framework;
using WB.Services.Export.CsvExport.Exporters;
using WB.Services.Export.Questionnaire;


namespace WB.Services.Export.Tests.CsvExport.Implementation.DoFiles
{
    [UseApprovalSubdirectory("../../QuestionsExportDoFiles-approved")]
    [IgnoreLineEndings(true)]
    [UseReporter(typeof(DiffReporter), typeof(NUnitReporter))]
    [TestOf(typeof(InterviewsDoFilesExporter))]
    internal class QuestionsExportDoFiles : QuestionsExportDoFilesContext
    {
        [Test]
        public void when_two_text_question_should_create_correct_do_file()
        {
            string stataGeneratedContent = "";

            var exportStructure = CreateQuestionnaireExportStructure(new IQuestionnaireEntity[]
            {
                Create.TextQuestion(variable: "textQuestion1", questionText: "questionText #1"),
                Create.TextQuestion(variable: "textQuestion2", questionText: "questionText #2", variableLabel: "variable Label #2"),
            });
            var exporter = CreateInterviewsDoFilesExporter(s => stataGeneratedContent = s);

            exporter.ExportDoFiles(exportStructure, "", CancellationToken.None);

            Approvals.Verify(stataGeneratedContent);
        }

        [Test]
        public void when_two_list_question_should_create_correct_do_file()
        {
            string stataGeneratedContent = "";

            var exportStructure = CreateQuestionnaireExportStructure(new IQuestionnaireEntity[]
            {
                Create.TextListQuestion(variable: "textListQuestion1", text: "questionText #1", maxAnswersCount: 10),
                Create.TextListQuestion(variable: "textListQuestion2", text: "questionText #2", label: "variable Label #22", maxAnswersCount: 5),
            });
            var exporter = CreateInterviewsDoFilesExporter(s => stataGeneratedContent = s);

            exporter.ExportDoFiles(exportStructure, "", CancellationToken.None);

            Approvals.Verify(stataGeneratedContent);
        }
    }
}
