using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Headquarters.DataExport.Denormalizers;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Factories.ExportViewFactoryTests
{
    internal class when_creating_export_structure_from_questionnaire_with_multi_option_question : ExportViewFactoryTestsContext
    {
        Establish context = () =>
        {
            multiOptionQuestion = Guid.NewGuid();
            questionnaireDocument =
                CreateQuestionnaireDocumentWithOneChapter(Create.MultyOptionsQuestion(id: multiOptionQuestion, variable: "mul",
                    answers:
                        new[]
                        {
                            Create.Answer("-23", -23), Create.Answer("70.3", (decimal)70.3), Create.Answer("-44.4", (decimal) -44.4),
                            Create.Answer("2", 2)
                        }));
            exportViewFactory = CreateExportViewFactory();
        };

        Because of = () =>
        {
            questionnaireExportStructure =
                exportViewFactory.CreateQuestionnaireExportStructure(questionnaireDocument, 1);
            multiOptionQuestionColumnNames =
                questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid>()].HeaderItems[multiOptionQuestion]
                    .ColumnNames;
        };

        It should_create_header_where_negative_sign_and_decimal_separator_of_a_multioption_question_value_replaced_with_n_and_underscore_respectively = () =>
            multiOptionQuestionColumnNames.ShouldEqual(new[] { "mul__n23", "mul__70_3", "mul__n44_4", "mul__2" });

        private static QuestionnaireExportStructure questionnaireExportStructure;
        private static ExportViewFactory exportViewFactory;
        private static Guid multiOptionQuestion;
        private static QuestionnaireDocument questionnaireDocument;
        private static string[] multiOptionQuestionColumnNames;
    }
}