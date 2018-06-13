using System;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Denormalizers;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Factories.ExportViewFactoryTests
{
    internal class when_creating_export_structure_from_questionnaire_with_multi_option_question : ExportViewFactoryTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            multiOptionQuestion = Guid.NewGuid();
            questionnaireDocument =
                CreateQuestionnaireDocumentWithOneChapter(Create.Entity.MultyOptionsQuestion(id: multiOptionQuestion, variable: "mul",
                    options:
                        new[]
                        {
                            Create.Entity.Answer("-23", -23), Create.Entity.Answer("70", 70), Create.Entity.Answer("-44", -44),
                            Create.Entity.Answer("2", 2)
                        }));

            var questionnaireMockStorage = new Mock<IQuestionnaireStorage>();
            questionnaireMockStorage.Setup(x => x.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>(), Moq.It.IsAny<string>())).Returns(Create.Entity.PlainQuestionnaire(questionnaireDocument, 1, null));
            questionnaireMockStorage.Setup(x => x.GetQuestionnaireDocument(Moq.It.IsAny<QuestionnaireIdentity>())).Returns(questionnaireDocument);
            exportViewFactory = CreateExportViewFactory(questionnaireMockStorage.Object);
            BecauseOf();
        }

        public void BecauseOf() 
        {
            questionnaireExportStructure =
                exportViewFactory.CreateQuestionnaireExportStructure(questionnaireDocument.PublicKey, 1);

            multiOptionQuestionColumnNames =
                questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid>()].HeaderItems[multiOptionQuestion]
                    .ColumnHeaders.Select(x => x.Name).ToArray();
        }

        [NUnit.Framework.Test] public void should_create_header_where_negative_sign_and_decimal_separator_of_a_multioption_question_value_replaced_with_n_and_underscore_respectively () =>
            multiOptionQuestionColumnNames.Should().BeEquivalentTo(new[] { "mul__n23", "mul__70", "mul__n44", "mul__2" });

        private static QuestionnaireExportStructure questionnaireExportStructure;
        private static ExportViewFactory exportViewFactory;
        private static Guid multiOptionQuestion;
        private static QuestionnaireDocument questionnaireDocument;
        private static string[] multiOptionQuestionColumnNames;
    }
}
