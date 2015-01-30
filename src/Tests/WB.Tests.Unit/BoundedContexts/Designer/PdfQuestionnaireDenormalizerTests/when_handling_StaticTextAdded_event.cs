using System;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Designer.PdfQuestionnaireDenormalizerTests
{
    internal class when_handling_StaticTextAdded_event : PdfQuestionnaireDenormalizerTestContext
    {
        Establish context = () =>
        {
            pdfQuestionnaireDocument = CreatePdfQuestionnaire(CreateGroup(Guid.Parse(parentId)));

            var documentStorage =
                Mock.Of<IReadSideKeyValueStorage<PdfQuestionnaireView>>(
                    writer => writer.GetById(Moq.It.IsAny<string>()) == pdfQuestionnaireDocument);

            denormalizer = CreatePdfQuestionnaireDenormalizer(documentStorage: documentStorage);
        };

        Because of = () =>
            denormalizer.Handle(Create.StaticTextAddedEvent(entityId: entityId, parentId: parentId, text: text));

        It should_static_text_not_be_null = () =>
            GetexpectedStaticText().ShouldNotBeNull();

        It should_title_of_static_text_be_equal_to_specified_text = () =>
            GetexpectedStaticText().Title.ShouldEqual(text);

        private static PdfStaticTextView GetexpectedStaticText()
        {
            return pdfQuestionnaireDocument.GetEntityById<PdfStaticTextView>(Guid.Parse(entityId));
        }

        private static PdfQuestionnaireDenormalizer denormalizer;
        private static string entityId = "11111111111111111111111111111111";
        private static string parentId = "22222222222222222222222222222222";
        private static string text = "some text";
        private static PdfQuestionnaireView pdfQuestionnaireDocument;
    }
}
