using System;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Designer.PdfQuestionnaireDenormalizerTests
{
    internal class when_handling_StaticTextCloned_event : PdfQuestionnaireDenormalizerTestContext
    {
        Establish context = () =>
        {
            var pdfGroupView = CreateGroup(parentId);
            pdfGroupView.AddChild(CreateStaticText(sourceEntityId));
            pdfQuestionnaireDocument = CreatePdfQuestionnaire(pdfGroupView);

            var documentStorage =
                Mock.Of<IReadSideKeyValueStorage<PdfQuestionnaireView>>(
                    writer => writer.GetById(Moq.It.IsAny<string>()) == pdfQuestionnaireDocument);

            denormalizer = CreatePdfQuestionnaireDenormalizer(documentStorage: documentStorage);
        };

        Because of = () =>
            denormalizer.Handle(Create.StaticTextClonedEvent(entityId: targetEntityId.ToString(),
                sourceEntityId: sourceEntityId.ToString(), parentId: parentId.ToString(), text: text,
                targetIndex: targetIndex));

        It should_static_text_not_be_null = () =>
            GetExpectedStaticText().ShouldNotBeNull();

        It should_title_of_static_text_be_equal_to_specified_text = () =>
            GetExpectedStaticText().Title.ShouldEqual(text);

        It should_parent_of_static_text_contains_2_entities = () =>
            GetParentOfexpectedStaticText().Children.Count.ShouldEqual(2);

        It should_index_of_cloned_static_text_be_equal_to_target_index = () =>
            GetParentOfexpectedStaticText().Children[0].PublicId.ShouldEqual(targetEntityId);

        private static PdfStaticTextView GetExpectedStaticText()
        {
            return pdfQuestionnaireDocument.GetEntityById<PdfStaticTextView>(targetEntityId);
        }

        private static PdfGroupView GetParentOfexpectedStaticText()
        {
            return pdfQuestionnaireDocument.Children[0] as PdfGroupView;
        }

        private static PdfQuestionnaireDenormalizer denormalizer;
        private static Guid sourceEntityId = Guid.Parse("11111111111111111111111111111111");
        private static Guid targetEntityId = Guid.Parse("33333333333333333333333333333333");
        private static Guid parentId = Guid.Parse("22222222222222222222222222222222");
        private static string text = "some text";
        private static int targetIndex = 0;
        private static PdfQuestionnaireView pdfQuestionnaireDocument;
    }
}
