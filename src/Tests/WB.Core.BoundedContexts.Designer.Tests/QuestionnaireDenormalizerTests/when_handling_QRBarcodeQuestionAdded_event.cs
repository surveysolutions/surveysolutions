using System;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Document;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireDenormalizerTests
{
    [Ignore("Will be finished later")]
    internal class when_handling_QRBarcodeQuestionAdded_event : QuestionnaireDenormalizerTestsContext
    {
        Establish context = () =>
        {
            @event = CreateQRBarcodeQuestionAddedEvent(questionId: questionId, parentGroupId: parentGroupId, variableName: variableName,
                title: title, isMandatory: isMandatory, conditionExpression: condition, instructions: instructions);

            var questionnaireDocument = CreateQuestionnaireDocument(new[]
            {
                CreateGroup(groupId: parentGroupId)
            });

            var documentStorage = new Mock<IReadSideRepositoryWriter<QuestionnaireDocument>>();
            documentStorage.Setup(writer => writer.GetById(Moq.It.IsAny<string>())).Returns(questionnaireDocument);
            documentStorage.Setup(writer => writer.Store(Moq.It.IsAny<QuestionnaireDocument>(), Moq.It.IsAny<Guid>())).Callback(
                (QuestionnaireDocument document, int eventSourceId) =>
                {
                    questionData =
                        document.FirstOrDefault<IQRBarcodeQuestion>(
                            question => question.PublicKey == questionId);
                });

            denormalizer = CreateQuestionnaireDenormalizer(documentStorage: documentStorage.Object);
        };

        Because of = () =>
            denormalizer.Handle(@event);

        It should_set_null_as_default_value_for__ValidationExpression__field = () =>
           questionData.ValidationExpression.ShouldBeNull();

        It should_set_null_as_default_value_for__ValidationMessage__field = () =>
            questionData.ValidationMessage.ShouldBeNull();

        It should_set_Interviewer_as_default_value_for__QuestionScope__field = () =>
            questionData.QuestionScope.ShouldEqual(QuestionScope.Interviewer);

        It should_set_false_as_default_value_for__Featured__field = () =>
            questionData.Featured.ShouldBeFalse();

        It should_set_TextList_as_default_value_for__QuestionType__field = () =>
            questionData.QuestionType.ShouldEqual(QuestionType.QRBarcode);

        private static IQRBarcodeQuestion questionData;
        private static QuestionnaireDenormalizer denormalizer;
        private static IPublishedEvent<QRBarcodeQuestionAdded> @event;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid parentGroupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static string variableName = "qr_barcode_question";
        private static bool isMandatory = true;
        private static string title = "title";
        private static string instructions = "intructions";
        private static string condition = "condition";
    }
}