using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
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
    internal class when_creating_export_structure_from_questionnaire_with_multi_option_question : ExportViewFactoryTestsContext
    {
        Establish context = () =>
        {
            multiOptionQuestion = Guid.NewGuid();
            questionnaireDocument =
                CreateQuestionnaireDocumentWithOneChapter(Create.Entity.MultyOptionsQuestion(id: multiOptionQuestion, variable: "mul",
                    options:
                        new[]
                        {
                            Create.Entity.Answer("-23", -23), Create.Entity.Answer("70.3", (decimal)70.3), Create.Entity.Answer("-44.4", (decimal) -44.4),
                            Create.Entity.Answer("2", 2)
                        }));

            var questionnaireMockStorage = new Mock<IQuestionnaireStorage>();
            questionnaireMockStorage.Setup(x => x.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>(), Moq.It.IsAny<string>())).Returns(new PlainQuestionnaire(questionnaireDocument, 1, null));
            questionnaireMockStorage.Setup(x => x.GetQuestionnaireDocument(Moq.It.IsAny<QuestionnaireIdentity>())).Returns(questionnaireDocument);
            exportViewFactory = CreateExportViewFactory(questionnaireMockStorage.Object);
        };

        Because of = () =>
        {
            questionnaireExportStructure =
                exportViewFactory.CreateQuestionnaireExportStructure(questionnaireDocument.PublicKey, 1);
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