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
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Services;
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

        internal static QuestionChanged CreateQuestionChangedEvent(Guid questionId, QuestionType type = QuestionType.Text, int maxValue = 0, List<Guid> triggers = null)
        {
            return new QuestionChanged
            {
                QuestionText = "What is your name",
                QuestionType = type,
                PublicKey = questionId,
                Featured = true,
                AnswerOrder = Order.AsIs,
                ConditionExpression = string.Empty,
                Answers = null,
                Instructions = "Answer this question, please",
                StataExportCaption = "name",
                ValidationExpression = "[this]!=''",
                ValidationMessage = "Empty names is invalid answer",
                Triggers = triggers,
            };
        }

        internal static GroupUpdated CreateGroupUpdatedEvent(Guid groupId, Propagate propagationKind = Propagate.None)
        {
            return new GroupUpdated
            {
                GroupPublicKey = groupId
            };
        }

        internal static IPublishedEvent<T> CreatePublishedEvent<T>(Guid questionnaireId, T evnt)
        {
            IPublishedEvent<T> e = new PublishedEvent<T>(new UncommittedEvent(Guid.NewGuid(),
                questionnaireId,
                1,
                1,
                DateTime.Now,
                evnt)
                );
            return e;
        }
    }
}