using System;
using System.Collections.Generic;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Moq;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Document;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireDenormalizerTests
{
    internal class QuestionChangeEventTestContext
    {
        internal static QuestionnaireDenormalizer CreateQuestionnaireDenormalizer(IQuestionnaireEntityFactory questionnaireEntityFactoryMock, Mock<IReadSideKeyValueStorage<QuestionnaireDocument>> storageStub)
        {
            var denormalizer = new QuestionnaireDenormalizer(storageStub.Object, questionnaireEntityFactoryMock, Mock.Of<ILogger>());

            return denormalizer;
        }

        internal static Mock<IReadSideRepositoryWriter<QuestionnaireDocument>> CreateQuestionnaireDenormalizerStorageStub(QuestionnaireDocument document)
        {
            var storageStub = new Mock<IReadSideRepositoryWriter<QuestionnaireDocument>>();

            storageStub.Setup(d => d.GetById(document.PublicKey.FormatGuid())).Returns(document);

            return storageStub;
        }

        internal static QuestionnaireDocument CreateQuestionnaireDocument(Guid questionnaireId)
        {
            var innerDocument = new QuestionnaireDocument
            {
                Title = string.Format("Questionnaire {0}", questionnaireId),
                PublicKey = questionnaireId
            };
            return innerDocument;
        }

        /*internal static QuestionChanged CreateQuestionChangedEvent(Guid questionId, QuestionType type = QuestionType.Text, int maxValue = 0)
        {
            return new QuestionChanged
            (
                questionText : "What is your name",
                questionType : type,
                publicKey : questionId,
                featured : true,
                answerOrder : Order.AsIs,
                conditionExpression : string.Empty,
                answers : null,
                instructions : "Answer this question, please",
                stataExportCaption : "name",
                validationExpression : "[this]!=''",
                validationMessage : "Empty names is invalid answer"
            );
        }*/

        internal static IPublishedEvent<T> CreatePublishedEvent<T>(Guid questionnaireId, T evnt)
            where T: ILiteEvent
        {
            return new PublishedEvent<T>(Create.PublishableEvent(eventSourceId: questionnaireId, payload: evnt));
        }
    }
}