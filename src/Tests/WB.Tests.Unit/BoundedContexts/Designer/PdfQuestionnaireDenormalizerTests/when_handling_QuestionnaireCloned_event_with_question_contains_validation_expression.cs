using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Designer.PdfQuestionnaireDenormalizerTests
{
    internal class when_handling_QuestionnaireCloned_event_with_question_contains_validation_expression : PdfQuestionnaireDenormalizerTestContext
    {
        Establish context = () =>
        {
            var parentGroupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            var questionnaireDocument = Create.QuestionnaireDocument(children: new[]
            {
                Create.Chapter(children: new[]
                {
                    Create.Question(enablementCondition: null, validationExpression: null),
                    Create.Question(questionId: questionId, enablementCondition: "enablementCondition of question", validationExpression: "validationExpression of question"),
                })
            });

            @event = CreatePublishedEvent(new QuestionnaireCloned
            {
                QuestionnaireDocument = questionnaireDocument
            });

            var documentStorageMock = new Mock<IReadSideKeyValueStorage<PdfQuestionnaireView>>();
            
            documentStorageMock
                .Setup(x => x.Store(Moq.It.IsAny<PdfQuestionnaireView>(), Moq.It.IsAny<string>()))
                .Callback((PdfQuestionnaireView pdf, string id) => pdfQuestionnaireDocument = pdf);

            denormalizer = CreatePdfQuestionnaireDenormalizer(documentStorage: documentStorageMock.Object);
        };

        Because of = () =>
            denormalizer.Handle(@event);

        It should_set_TextList_question_title_be_equal_to_passed_title = () =>
            pdfQuestionnaireDocument.GetEntityById<PdfQuestionView>(questionId).ValidationExpression.ShouldEqual("validationExpression of question");

         It should_set_TextList_question_title_be_equal_to_passed_condition = () =>
            pdfQuestionnaireDocument.GetEntityById<PdfQuestionView>(questionId).ConditionExpression.ShouldEqual( "enablementCondition of question");

        private static PdfQuestionnaireDenormalizer denormalizer;
        private static IPublishedEvent<QuestionnaireCloned> @event;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static PdfQuestionnaireView pdfQuestionnaireDocument;
    }
}