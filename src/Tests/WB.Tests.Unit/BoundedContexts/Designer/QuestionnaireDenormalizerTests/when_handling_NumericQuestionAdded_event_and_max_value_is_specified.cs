using System;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Document;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireDenormalizerTests
{
    internal class when_handling_NumericQuestionAdded_event_and_max_value_is_specified : QuestionnaireDenormalizerTestsContext
    {
        Establish context = () =>
        {
            var parentGroupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            questionId = Guid.Parse("11111111111111111111111111111111");
            maxValue = 42;

            questionnaireDocument = CreateQuestionnaireDocument(
                CreateGroup(groupId: parentGroupId)
            );

            @event = CreateNumericQuestionAddedEvent(questionId: questionId, parentGroupId: parentGroupId, maxValue: maxValue);

            var documentStorage = Mock.Of<IReadSideKeyValueStorage<QuestionnaireDocument>>(writer
                => writer.GetById(it.IsAny<string>()) == questionnaireDocument);

            var textQuestion = CreateNumericQuestion(questionId: questionId, maxValue: maxValue);

            var questionFactory = Mock.Of<IQuestionnaireEntityFactory>(x =>
                x.CreateQuestion(Moq.It.IsAny<QuestionData>()) == textQuestion
            );

            denormalizer = CreateQuestionnaireDenormalizer(documentStorage: documentStorage, questionnaireEntityFactory: questionFactory);
        };

        Because of = () =>
            denormalizer.Handle(@event);

        It should_set_question_MaxValue_property_to_specified_max_value = () =>
            questionnaireDocument.GetQuestion<INumericQuestion>(questionId)
                .MaxValue.ShouldEqual(maxValue);

        private static QuestionnaireDenormalizer denormalizer;
        private static IPublishedEvent<NumericQuestionAdded> @event;
        private static QuestionnaireDocument questionnaireDocument;
        private static Guid questionId;
        private static int maxValue;
    }
}