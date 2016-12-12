using System;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Denormalizers;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Factories.ExportViewFactoryTests
{
    internal class when_question_text_contains_html : ExportViewFactoryTestsContext
    {
        Establish context = () =>
        {
            questionnaireDocument = CreateQuestionnaireDocumentWithOneChapter(
                Create.Entity.NumericIntegerQuestion(
                    id: questionWithHtml,
                    questionText: "with <strong>html</stong>"
                    )
                 );

            var questionnaireMockStorage = new Mock<IQuestionnaireStorage>();
            questionnaireMockStorage.Setup(x => x.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>(), Moq.It.IsAny<string>()))
                                    .Returns(new PlainQuestionnaire(questionnaireDocument, 1, null));
            questionnaireMockStorage.Setup(x => x.GetQuestionnaireDocument(Moq.It.IsAny<QuestionnaireIdentity>()))
                                    .Returns(questionnaireDocument);
            exportViewFactory = CreateExportViewFactory(questionnaireMockStorage.Object);
        };

        Because of = () =>
            questionnaireExportStructure = exportViewFactory.CreateQuestionnaireExportStructure(new QuestionnaireIdentity(questionnaireDocument.PublicKey, 1));

        It should_cut_html_tags_from_header = () =>
             questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid>()].HeaderItems[questionWithHtml].Titles[0].ShouldEqual("with html");

        static QuestionnaireExportStructure questionnaireExportStructure;
        static ExportViewFactory exportViewFactory;
        static readonly Guid questionWithHtml = Guid.Parse("CCF000AAA111EE2DD2EE111AAA000FFF");
        static QuestionnaireDocument questionnaireDocument;
    }
}
