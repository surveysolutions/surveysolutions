using System;
using System.Linq;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.PdfQuestionnaireDenormalizerTests
{
    internal class when_handling_MacroDeleted_event : PdfQuestionnaireDenormalizerTestContext
    {
        Establish context = () =>
        {
            pdfQuestionnaireDocument = CreatePdfQuestionnaire();

            var documentStorage = Mock.Of<IReadSideKeyValueStorage<PdfQuestionnaireView>>(
                writer => writer.GetById(Moq.It.IsAny<string>()) == pdfQuestionnaireDocument);

            denormalizer = CreatePdfQuestionnaireDenormalizer(documentStorage: documentStorage);
            denormalizer.Handle(Create.Event.MacroAdded(questionnaireId, entityId));
        };

        Because of = () =>
            denormalizer.Handle(Create.Event.MacroDeleted(questionnaireId, entityId));

        It should_delete_one_macro = () =>
            pdfQuestionnaireDocument.GetMacros().Count().ShouldEqual(0);

        private static PdfQuestionnaireDenormalizer denormalizer;
        private static Guid entityId = Guid.Parse("11111111111111111111111111111111");
        private static Guid questionnaireId = Guid.Parse("22222222222222222222222222222222");
        private static PdfQuestionnaireView pdfQuestionnaireDocument;
    }
}