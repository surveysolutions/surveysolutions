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
    internal class when_interview_has_yesno_question_with_not_sorted_answers : ExportViewFactoryTests
    {
        [NUnit.Framework.OneTimeSetUp]
        public void context()
        {
            questionId = Guid.Parse("d7127d06-5668-4fa3-b255-8a2a0aaaa020");
            variableName = "yesno";
            questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(
                Create.MultyOptionsQuestion(id: questionId,
                    variable: variableName,
                    options: new List<Answer> {
                        Create.Answer("foo", 28),
                        Create.Answer("bar", 42),
                        Create.Answer("blah", 21),
                        Create.Answer("bar_null", 15)
                    }, areAnswersOrdered: false,
                    yesNoView: true));

            questionnaaireExportStructure = Create.QuestionnaireExportStructure(questionnaireDocument);

            var answeredYesNoOptions = new[] {
                Create.AnsweredYesNoOption(21m, true),
                Create.AnsweredYesNoOption(42m, false),
                Create.AnsweredYesNoOption(28m, true)

            };
            interview = CreateInterviewData(Create.InterviewEntity(identity: Create.Identity(questionId),
                asYesNo: answeredYesNoOptions
            ));

            exporter = Create.InterviewsExporter();

            BecauseOf();
        }

        private void BecauseOf() => result = exporter.CreateInterviewDataExportView(questionnaaireExportStructure, interview, questionnaireDocument);

        [NUnit.Framework.Test]
        public void should_fill_yesno_question_answer_without_order()
        {
            InterviewDataExportLevelView first = result.Levels.First();
            var exportedQuestion = first.Records.First().GetPlainAnswers().First();
            exportedQuestion.Length.Should().Be(4);
            exportedQuestion.Should().BeEquivalentTo(new[] { "1", "0", "1", ExportFormatSettings.MissingNumericQuestionValue }); // 1 0 1
        }

        static QuestionnaireExportStructureFactory QuestionnaireExportStructureFactory;
        static QuestionnaireExportStructure questionnaaireExportStructure;
        static Guid questionId;
        static InterviewData interview;
        static InterviewDataExportView result;
        static string variableName;
        private InterviewsExporter exporter;
        private QuestionnaireDocument questionnaireDocument;
    }
}
