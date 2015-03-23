using System;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Designer.PdfQuestionnaireDenormalizerTests
{
    internal class when_handling_StaticTextDeleted_event : PdfQuestionnaireDenormalizerTestContext
    {
        Establish context = () =>
        {
            var pdfGroupView = CreateGroup(parentId);
            pdfGroupView.AddChild(CreateStaticText(entityId));
            pdfQuestionnaireDocument = CreatePdfQuestionnaire(pdfGroupView);

            var documentStorage =
                Mock.Of<IReadSideKeyValueStorage<PdfQuestionnaireView>>(
                    writer => writer.GetById(Moq.It.IsAny<string>()) == pdfQuestionnaireDocument);

            denormalizer = CreatePdfQuestionnaireDenormalizer(documentStorage: documentStorage);
        };

        Because of = () =>
            denormalizer.Handle(Create.StaticTextDeletedEvent(entityId: entityId.ToString()));

        It should_static_text_does_not_exists = () =>
            GetExpectedStaticText().ShouldBeNull();

        private static PdfStaticTextView GetExpectedStaticText()
        {
            return pdfQuestionnaireDocument.GetEntityById<PdfStaticTextView>(entityId);
        }

        private static PdfGroupView GetParentOfexpectedStaticText()
        {
            return pdfQuestionnaireDocument.Children[0] as PdfGroupView;
        }

        private static PdfQuestionnaireDenormalizer denormalizer;
        private static Guid entityId = Guid.Parse("33333333333333333333333333333333");
        private static Guid parentId = Guid.Parse("22222222222222222222222222222222");
        private static PdfQuestionnaireView pdfQuestionnaireDocument;
    }
}
