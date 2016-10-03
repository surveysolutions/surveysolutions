using System;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Denormalizers;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Factories.ExportViewFactoryTests
{
    internal class when_creating_export_structure_from_questionnaire_containing_2_questions_one_with_var_lable_other_without : ExportViewFactoryTestsContext
    {
        Establish context = () =>
        {
            questionnaireDocument = CreateQuestionnaireDocumentWithOneChapter(
                 new NumericQuestion() { PublicKey = questionWithVariableLabelId, QuestionType = QuestionType.Numeric, VariableLabel = variableLabel, QuestionText = "text"},
                 new NumericQuestion() { PublicKey = questionWithoutVariableLabelId, QuestionType = QuestionType.Numeric, QuestionText = questionText}
                 );

            var questionnaireMockStorage = new Mock<IQuestionnaireStorage>();
            questionnaireMockStorage.Setup(x => x.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>(), Moq.It.IsAny<string>())).Returns(new PlainQuestionnaire(questionnaireDocument, 1, null));
            questionnaireMockStorage.Setup(x => x.GetQuestionnaireDocument(Moq.It.IsAny<QuestionnaireIdentity>())).Returns(questionnaireDocument);
            exportViewFactory = CreateExportViewFactory(questionnaireMockStorage.Object);
        };

        Because of = () =>
            questionnaireExportStructure = exportViewFactory.CreateQuestionnaireExportStructure(new QuestionnaireIdentity(questionnaireDocument.PublicKey, 1));

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
