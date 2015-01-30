using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Designer.PdfQuestionnaireDenormalizerTests
{
    internal class when_handling_QuestionCloned_event_with_validation_expression : PdfQuestionnaireDenormalizerTestContext
    {
        Establish context = () =>
        {
            var parentGroupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            @event = CreatePublishedEvent(new QuestionCloned()
            {
                QuestionType = QuestionType.Text,
                PublicKey = questionId, 
                GroupPublicKey = parentGroupId, 
                QuestionText = "someTitle",
                ValidationExpression = "some expression"
            });

            pdfQuestionnaireDocument = CreatePdfQuestionnaire(CreateGroup(parentGroupId));

            var documentStorage = Mock.Of<IReadSideKeyValueStorage<PdfQuestionnaireView>>(writer => writer.GetById(Moq.It.IsAny<string>()) == pdfQuestionnaireDocument);

            denormalizer = CreatePdfQuestionnaireDenormalizer(documentStorage: documentStorage);
        };

        Because of = () =>
            denormalizer.Handle(@event);

        It should_set_Text_question_type_for_cloned_question = () =>
            pdfQuestionnaireDocument.GetEntityById<PdfQuestionView>(questionId).QuestionType.ShouldEqual(QuestionType.Text);

        It should_set__someTitle__as_title_for_cloned_question = () =>
            pdfQuestionnaireDocument.GetEntityById<PdfQuestionView>(questionId).Title.ShouldEqual("someTitle");

        It should_set__some_expression___as_validation_for_cloned_question = () =>
            pdfQuestionnaireDocument.GetEntityById<PdfQuestionView>(questionId).ValidationExpression.ShouldEqual("some expression");

        private static PdfQuestionnaireDenormalizer denormalizer;
        private static IPublishedEvent<QuestionCloned> @event;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static PdfQuestionnaireView pdfQuestionnaireDocument;
    }
}