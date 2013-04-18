using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Main.Core.Tests.Domain.QuestionnaireDenormalizerTests
{
    using Machine.Specifications;

    using Main.Core.AbstractFactories;
    using Main.Core.Documents;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Question;
    using Main.Core.EventHandlers;
    using Main.Core.Events.Questionnaire;
    using Main.DenormalizerStorage;

    using Moq;

    using Ncqrs.Eventing;
    using Ncqrs.Eventing.ServiceModel.Bus;

    [Subject(typeof(QuestionnaireDenormalizer))]
    internal class QuestionnaireDenormalizerTestsContext
    {
        protected static IPublishedEvent<T> ToPublishedEvent<T>(T @event)
            where T : class
        {
            return Mock.Of<IPublishedEvent<T>>(publishedEvent
                => publishedEvent.Payload == @event);
        }

        protected static QuestionnaireDenormalizer CreateQuestionnaireDenormalizer(
            IDenormalizerStorage<QuestionnaireDocument> documentStorage = null, ICompleteQuestionFactory questionFactory = null)
        {
            return new QuestionnaireDenormalizer(
                documentStorage ?? Mock.Of<IDenormalizerStorage<QuestionnaireDocument>>(),
                questionFactory ?? Mock.Of<ICompleteQuestionFactory>());
        }

        protected static QuestionnaireDocument CreateQuestionnaireDocument()
        {
            return new QuestionnaireDocument();
        }

        protected static Group CreateGroup(Guid? groupId = null, string title = "Group X")
        {
            return new Group
            {
                PublicKey = groupId ?? Guid.NewGuid(), 
                Title = title,
            };
        }

        protected static AbstractQuestion CreateQuestion(Guid? questionId = null, string title = null)
        {
            return new TextQuestion
            {
                PublicKey = questionId ?? Guid.NewGuid(),
                QuestionText = title,
            };
        }

        protected static IPublishedEvent<GroupDeleted> CreateGroupDeletedEvent(Guid groupId)
        {
            return ToPublishedEvent(new GroupDeleted
            {
                GroupPublicKey = groupId,
            });
        }

        protected static IPublishedEvent<QuestionDeleted> CreateQuestionDeletedEvent(Guid questionId)
        {
            return ToPublishedEvent(new QuestionDeleted
            {
                QuestionId = questionId,
            });
        }
    }
}
