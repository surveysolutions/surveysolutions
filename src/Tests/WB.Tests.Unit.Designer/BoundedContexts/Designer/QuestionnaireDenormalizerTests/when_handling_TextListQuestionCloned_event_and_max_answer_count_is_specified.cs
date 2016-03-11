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
    internal class when_handling_TextListQuestionCloned_event_and_max_answer_count_is_specified : QuestionnaireDenormalizerTestsContext
    {
        Establish context = () =>
        {
            var parentGroupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            maxAnswerCount = 4;

            questionnaireDocument = CreateQuestionnaireDocument(
                CreateGroup(groupId: parentGroupId, setup: group => group.RosterSizeQuestionId = null)
            );

            @event = TextListQuestionClonedEvent(questionId: questionId, parentGroupId: parentGroupId, maxAnswerCount: maxAnswerCount);

            var documentStorage = Mock.Of<IReadSideKeyValueStorage<QuestionnaireDocument>>(writer
                => writer.GetById(it.IsAny<string>()) == questionnaireDocument);

            var questionFactory = new Mock<IQuestionnaireEntityFactory>();

            var clonedQuestion = CreateTextListQuestion(questionId: questionId);

            questionFactory.Setup(x => x.CreateQuestion(Moq.It.IsAny<QuestionData>()))
                .Callback<QuestionData>(d => questionData = d)
                .Returns(() => clonedQuestion);

            denormalizer = CreateQuestionnaireDenormalizer(documentStorage: documentStorage, questionnaireEntityFactory: questionFactory.Object);
        };

        Because of = () =>
            denormalizer.Handle(@event);

        private It should_set_not_null_questions_MaxAnswerCount_property = () =>
            questionData.MaxAnswerCount.ShouldNotBeNull();

        private It should_set_question_MaxValue_property_to_specified_max_value = () =>
            questionData.MaxAnswerCount.ShouldEqual(maxAnswerCount);

        private static QuestionnaireDenormalizer denormalizer;
        private static IPublishedEvent<TextListQuestionCloned> @event;
        private static QuestionnaireDocument questionnaireDocument;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static int maxAnswerCount;
        private static QuestionData questionData;
    }
}