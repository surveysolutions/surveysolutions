using System;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Designer.PdfQuestionnaireDenormalizerTests
{
    internal class when_handling_StaticTextUpdated_event : PdfQuestionnaireDenormalizerTestContext
    {
        Establish context = () =>
        {
            var pdfGroupView = CreateGroup(Guid.Parse(parentId));
            pdfGroupView.AddChild(CreateStaticText(Guid.Parse(entityId)));
            pdfQuestionnaireDocument = CreatePdfQuestionnaire(pdfGroupView);

            var documentStorage =
                Mock.Of<IReadSideKeyValueStorage<PdfQuestionnaireView>>(
                    writer => writer.GetById(Moq.It.IsAny<string>()) == pdfQuestionnaireDocument);

            denormalizer = CreatePdfQuestionnaireDenormalizer(documentStorage: documentStorage);
        };

        Because of = () =>
            denormalizer.Handle(Create.StaticTextUpdatedEvent(entityId: entityId, text: text));

        It should_static_text_not_be_null = () =>
            GetExpectedStaticText().ShouldNotBeNull();

        It should_title_of_static_text_be_equal_to_specified_title = () =>
            GetExpectedStaticText().Title.ShouldEqual(text);

        private static PdfStaticTextView GetExpectedStaticText()
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