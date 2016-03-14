using System;
using System.Linq;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.PdfQuestionnaireDenormalizerTests
{
    internal class when_handling_MacroUpdated_event : PdfQuestionnaireDenormalizerTestContext
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
            denormalizer.Handle(Create.Event.MacroUpdated(questionnaireId, entityId, name, content, description));

        It should_update_macro_with_specified_name = () =>
            pdfQuestionnaireDocument.GetMacros().First().Name.ShouldEqual(name);

        It should_update_macro_with_specifiedy_content = () =>
            pdfQuestionnaireDocument.GetMacros().First().Content.ShouldEqual(content);

        It should_update_macro_with_specified_description = () =>
            pdfQuestionnaireDocument.GetMacros().First().Description.ShouldEqual(description);

        private static PdfQuestionnaireDenormalizer denormalizer;
        private static Guid entityId = Guid.Parse("11111111111111111111111111111111");
        private static Guid questionnaireId = Guid.Parse("22222222222222222222222222222222");
        private static readonly string name = "macros";
        private static readonly string content = "macros content";
        private static readonly string description = "macros description";
        private static PdfQuestionnaireView pdfQuestionnaireDocument;
    }
}