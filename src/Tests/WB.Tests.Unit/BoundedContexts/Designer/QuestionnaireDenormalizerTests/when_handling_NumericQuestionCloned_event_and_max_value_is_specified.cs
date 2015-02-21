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
    internal class when_handling_NumericQuestionCloned_event_and_max_value_is_specified : QuestionnaireDenormalizerTestsContext
    {
        Establish context = () =>
        {
            var parentGroupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            maxValue = 42;

            questionnaireDocument = CreateQuestionnaireDocument(
                CreateGroup(groupId: parentGroupId, setup: group => group.RosterSizeQuestionId = null)
            );

            @event = CreateNumericQuestionClonedEvent(questionId: questionId, parentGroupId: parentGroupId, maxValue: maxValue);

            var documentStorage = Mock.Of<IReadSideKeyValueStorage<QuestionnaireDocument>>(writer
                => writer.GetById(it.IsAny<string>()) == questionnaireDocument);

            var numericQuestion = CreateNumericQuestion(questionId, "title", maxValue);

            var questionFactory = Mock.Of<IQuestionnaireEntityFactory>(factory => factory.CreateQuestion(it.IsAny<QuestionData>()) == numericQuestion);

            denormalizer = CreateQuestionnaireDenormalizer(documentStorage: documentStorage, questionnaireEntityFactory: questionFactory);
        };

        Because of = () =>
            denormalizer.Handle(@event);

        It should_set_question_MaxValue_property_to_specified_max_value = () =>
            questionnaireDocument.GetQuestion<INumericQuestion>(questionId)
                .MaxValue.ShouldEqual(maxValue);

        private static QuestionnaireDenormalizer denormalizer;
        private static IPublishedEvent<NumericQuestionCloned> @event;
        private static QuestionnaireDocument questionnaireDocument;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static int maxValue;
    }
}