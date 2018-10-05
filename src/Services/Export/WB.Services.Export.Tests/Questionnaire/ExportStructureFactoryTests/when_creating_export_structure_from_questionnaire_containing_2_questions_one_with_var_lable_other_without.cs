using System;
using FluentAssertions;
using NUnit.Framework;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.Tests.Questionnaire.ExportStructureFactoryTests
{
    internal class
        when_creating_export_structure_from_questionnaire_containing_2_questions_one_with_var_lable_other_without :
            ExportViewFactoryTestsContext
    {
        private static QuestionnaireExportStructure questionnaireExportStructure;
        private static QuestionnaireExportStructureFactory QuestionnaireExportStructureFactory;
        private static readonly Guid questionWithVariableLabelId = Guid.Parse("CCF000AAA111EE2DD2EE111AAA000FFF");
        private static readonly Guid questionWithoutVariableLabelId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static QuestionnaireDocument questionnaireDocument;
        private static readonly string variableLabel = "var label";
        private static readonly string questionText = "question text";

        [OneTimeSetUp]
        public void context()
        {
            questionnaireDocument = CreateQuestionnaireDocumentWithOneChapter(
                new NumericQuestion
                {
                    PublicKey = questionWithVariableLabelId, QuestionType = QuestionType.Numeric,
                    VariableLabel = variableLabel, QuestionText = "text"
                },
                new NumericQuestion
                {
                    PublicKey = questionWithoutVariableLabelId, QuestionType = QuestionType.Numeric,
                    QuestionText = questionText
                }
            );

            QuestionnaireExportStructureFactory = CreateExportViewFactory();
            BecauseOf();
        }

        private void BecauseOf()
        {
            questionnaireExportStructure =
                QuestionnaireExportStructureFactory.CreateQuestionnaireExportStructure(questionnaireDocument);
        }

        [Test]
        public void should_create_header_with_title_equal_to_variable_lable_if_variable_label_is_not_empty()
        {
            questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid>()]
                .HeaderItems[questionWithVariableLabelId].ColumnHeaders[0].Title.Should().Be(variableLabel);
        }

        [Test]
        public void should_create_header_with_title_equal_to_question_title_if_variable_label_is_empty()
        {
            questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid>()]
                .HeaderItems[questionWithoutVariableLabelId].ColumnHeaders[0].Title.Should().Be(questionText);
        }
    }
}
