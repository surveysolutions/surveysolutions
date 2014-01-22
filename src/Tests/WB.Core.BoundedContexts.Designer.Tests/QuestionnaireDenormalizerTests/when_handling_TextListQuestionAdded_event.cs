using System;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities;
using Main.Core.Entities.SubEntities;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Document;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireDenormalizerTests
{
    internal class when_handling_TextListQuestionAdded_event : QuestionnaireDenormalizerTestsContext
    {
        Establish context = () =>
        {
            var parentGroupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            @event = CreateTextListQuestionAddedEvent(questionId: questionId, parentGroupId: parentGroupId);

            var questionnaireDocument = CreateQuestionnaireDocument(new[]
            {
                CreateGroup(groupId: parentGroupId)
            });

            var documentStorage = Mock.Of<IReadSideRepositoryWriter<QuestionnaireDocument>>(writer
                => writer.GetById(Moq.It.IsAny<Guid>()) == questionnaireDocument);

            questionFactory = new Mock<IQuestionFactory>();

            questionFactory.Setup(x => x.CreateQuestion(Moq.It.IsAny<QuestionData>()))
                .Callback<QuestionData>(d => questionData = d);

            denormalizer = CreateQuestionnaireDenormalizer(documentStorage: documentStorage, questionFactory: questionFactory.Object);
        };

        Because of = () =>
            denormalizer.Handle(@event);

        private It should_set_null_as_default_value_for__ValidationExpression__filed = () =>
            questionData.ValidationExpression.ShouldBeNull();

        private static QuestionData questionData;
        private static QuestionnaireDenormalizer denormalizer;
        private static IPublishedEvent<TextListQuestionAdded> @event;
        private static Mock<IQuestionFactory> questionFactory;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
    }
}