using System;
using System.Linq;

using Machine.Specifications;

using Moq;

using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Designer.PdfQuestionnaireDenormalizerTests
{
    internal class when_handling_MacroAdded_event : PdfQuestionnaireDenormalizerTestContext
    {
        Establish context = () =>
        {
            pdfQuestionnaireDocument = CreatePdfQuestionnaire();

            var documentStorage = Mock.Of<IReadSideKeyValueStorage<PdfQuestionnaireView>>(
                    writer => writer.GetById(Moq.It.IsAny<string>()) == pdfQuestionnaireDocument);

            denormalizer = CreatePdfQuestionnaireDenormalizer(documentStorage: documentStorage);
        };

        Because of = () =>
            denormalizer.Handle(Create.Event.MacroAdded(questionnaireId, entityId));

        It should_add_one_macro = () =>
            pdfQuestionnaireDocument.GetMacros().Count().ShouldEqual(1);

        It should_add_macro_with_empty_name = () =>
          pdfQuestionnaireDocument.GetMacros().First().Name.ShouldBeNull();

        It should_add_macro_with_empty_content = () =>
           pdfQuestionnaireDocument.GetMacros().First().Content.ShouldBeNull();

        It should_add_macro_with_empty_description = () =>
           pdfQuestionnaireDocument.GetMacros().First().Description.ShouldBeNull();

        private static PdfQuestionnaireDenormalizer denormalizer;
        private static Guid entityId = Guid.Parse("11111111111111111111111111111111");
        private static Guid questionnaireId = Guid.Parse("22222222222222222222222222222222");
        private static PdfQuestionnaireView pdfQuestionnaireDocument;
    }
}