using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.Factories.ExportViewFactoryTests
{
    internal class when_creating_export_structure_from_questionnaire_containing_2_questions_one_with_var_lable_other_without : ExportViewFactoryTestsContext
    {
        Establish context = () =>
        {
            questionnaireDocument = CreateQuestionnaireDocumentWithOneChapter(
                 new NumericQuestion() { PublicKey = questionWithVariableLabelId, QuestionType = QuestionType.Numeric, VariableLabel = variableLabel, QuestionText = "text"},
                 new NumericQuestion() { PublicKey = questionWithoutVariableLabelId, QuestionType = QuestionType.Numeric, QuestionText = questionText}
                 );
            exportViewFactory = CreateExportViewFactory();
        };

        Because of = () =>
            questionnaireExportStructure = exportViewFactory.CreateQuestionnaireExportStructure(questionnaireDocument, 1);

        It should_create_header_with_title_equal_to_variable_lable_if_variable_label_is_not_empty = () =>
            questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid>()].HeaderItems[questionWithVariableLabelId].Titles[0].ShouldEqual(variableLabel);

        It should_create_header_with_title_equal_to_question_title_if_variable_label_is_empty = () =>
          questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid>()].HeaderItems[questionWithoutVariableLabelId].Titles[0].ShouldEqual(questionText);

        private static QuestionnaireExportStructure questionnaireExportStructure;
        private static ExportViewFactory exportViewFactory;
        private static Guid questionWithVariableLabelId = Guid.Parse("CCF000AAA111EE2DD2EE111AAA000FFF");
        private static Guid questionWithoutVariableLabelId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static QuestionnaireDocument questionnaireDocument;
        private static string variableLabel = "var label";
        private static string questionText = "question text";
    }
}
