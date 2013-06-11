using WB.Core.Infrastructure;

namespace Main.Core.Tests.Domain.QuestionnaireDenormalizerTests
{
    using System;

    using Machine.Specifications;

    using Main.Core.AbstractFactories;
    using Main.Core.Documents;
    using Main.Core.Entities.SubEntities;
    using Main.Core.EventHandlers;
    using Main.Core.Events.Questionnaire;
    using Main.DenormalizerStorage;

    using Moq;

    using Ncqrs.Eventing.ServiceModel.Bus;

    using It = Machine.Specifications.It;
    using it = Moq.It;

    internal class when_updating_question_by_id_and_there_are_3_questions_with_same_id : QuestionnaireDenormalizerTestsContext
    {
        Establish context = () =>
        {
            var questionId = Guid.Parse("11111111111111111111111111111111");

            initialQuestionTitle = "Initial Title";
            updatedQuestionTitle = "Updated Title";

            questionUpdatedEvent = CreateQuestionChangedEvent(questionId: questionId, title: updatedQuestionTitle);

            QuestionnaireDocument questionnaire = CreateQuestionnaireDocument(children: new[] 
            {
                CreateGroup(children: new[]
                {
                    firstQuestion = CreateQuestion(questionId: questionId, title: initialQuestionTitle),
                    secondQuestion = CreateQuestion(questionId: questionId, title: initialQuestionTitle),
                    thirdQuestion = CreateQuestion(questionId: questionId, title: initialQuestionTitle),
                }),
            });

            var documentStorage = Mock.Of<IDenormalizerStorage<QuestionnaireDocument>>(storage
                => storage.GetById(it.IsAny<Guid>()) == questionnaire);

            var questionFactory = Mock.Of<ICompleteQuestionFactory>();
            Mock.Get(questionFactory)
                .Setup(factory => factory.CreateQuestionFromExistingUsingDataFromEvent(it.IsAny<IQuestion>(), it.IsAny<QuestionChanged>()))
                .Returns<IQuestion, QuestionChanged>((question, @event) =>
                {
                    question.QuestionText = @event.QuestionText;
                    return question;
                });

            denormalizer = CreateQuestionnaireDenormalizer(documentStorage: documentStorage, questionFactory: questionFactory);
        };

        Because of = () =>
            denormalizer.Handle(questionUpdatedEvent);

        It should_update_first_question = () =>
            firstQuestion.QuestionText.ShouldEqual(updatedQuestionTitle);

        It should_not_update_second_question = () =>
            secondQuestion.QuestionText.ShouldEqual(initialQuestionTitle);

        It should_not_update_third_question = () =>
            thirdQuestion.QuestionText.ShouldEqual(initialQuestionTitle);

        private static QuestionnaireDenormalizer denormalizer;
        private static IPublishedEvent<QuestionChanged> questionUpdatedEvent;
        private static AbstractQuestion firstQuestion;
        private static AbstractQuestion secondQuestion;
        private static AbstractQuestion thirdQuestion;
        private static string initialQuestionTitle;
        private static string updatedQuestionTitle;
    }
}