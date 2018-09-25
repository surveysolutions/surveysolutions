using System;
using FluentAssertions;
using Moq;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.Tests.Questionnaire.ExportStructureFactoryTests
{
    internal class when_question_text_contains_html : ExportViewFactoryTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaireDocument = CreateQuestionnaireDocumentWithOneChapter(
                Create.NumericIntegerQuestion(
                    id: questionWithHtml,
                    questionText: "with <strong>html</stong>"
                    )
                 );

            questionnaireExportStructureFactory = CreateExportViewFactory();
            BecauseOf();
        }

        public void BecauseOf() =>
            questionnaireExportStructure = questionnaireExportStructureFactory.CreateQuestionnaireExportStructure(questionnaireDocument);

        [NUnit.Framework.Test] public void should_cut_html_tags_from_header () =>
             questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid>()].HeaderItems[questionWithHtml].ColumnHeaders[0].Title.Should().Be("with html");

        static QuestionnaireExportStructure questionnaireExportStructure;
        static QuestionnaireExportStructureFactory questionnaireExportStructureFactory;
        static readonly Guid questionWithHtml = Guid.Parse("CCF000AAA111EE2DD2EE111AAA000FFF");
        static QuestionnaireDocument questionnaireDocument;
    }
}
