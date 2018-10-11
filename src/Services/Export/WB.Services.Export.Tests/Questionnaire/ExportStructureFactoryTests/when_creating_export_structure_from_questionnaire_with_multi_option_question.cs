using System;
using System.Linq;
using FluentAssertions;
using Moq;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.Tests.Questionnaire.ExportStructureFactoryTests
{
    internal class when_creating_export_structure_from_questionnaire_with_multi_option_question : ExportViewFactoryTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            multiOptionQuestion = Guid.NewGuid();
            questionnaireDocument =
                CreateQuestionnaireDocumentWithOneChapter(Create.MultyOptionsQuestion(id: multiOptionQuestion, variable: "mul",
                    options:
                        new[]
                        {
                            Create.Answer("-23", -23), Create.Answer("70", 70), Create.Answer("-44", -44),
                            Create.Answer("2", 2)
                        }));

            QuestionnaireExportStructureFactory = CreateExportViewFactory();
            BecauseOf();
        }

        public void BecauseOf() 
        {
            questionnaireExportStructure =
                QuestionnaireExportStructureFactory.CreateQuestionnaireExportStructure(questionnaireDocument);

            multiOptionQuestionColumnNames =
                questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid>()].HeaderItems[multiOptionQuestion]
                    .ColumnHeaders.Select(x => x.Name).ToArray();
        }

        [NUnit.Framework.Test] public void should_create_header_where_negative_sign_and_decimal_separator_of_a_multioption_question_value_replaced_with_n_and_underscore_respectively () =>
            multiOptionQuestionColumnNames.Should().BeEquivalentTo(new[] { "mul__n23", "mul__70", "mul__n44", "mul__2" });

        private static QuestionnaireExportStructure questionnaireExportStructure;
        private static QuestionnaireExportStructureFactory QuestionnaireExportStructureFactory;
        private static Guid multiOptionQuestion;
        private static QuestionnaireDocument questionnaireDocument;
        private static string[] multiOptionQuestionColumnNames;
    }
}
