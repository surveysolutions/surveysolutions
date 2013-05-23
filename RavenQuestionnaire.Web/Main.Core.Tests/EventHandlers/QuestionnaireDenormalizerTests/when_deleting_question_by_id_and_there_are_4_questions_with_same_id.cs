namespace Main.Core.Tests.Domain.QuestionnaireDenormalizerTests
{
    using System;

    using Machine.Specifications;

    using Main.Core.Documents;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Question;
    using Main.Core.EventHandlers;
    using Main.Core.Events.Questionnaire;
    using Main.DenormalizerStorage;

    using Moq;

    using Ncqrs.Eventing.ServiceModel.Bus;

    using It = Machine.Specifications.It;
    using it = Moq.It;

    internal class when_deleting_question_by_id_and_there_are_4_questions_with_same_id : QuestionnaireDenormalizerTestsContext
    {
        Establish context = () =>
        {
            var questionId = Guid.Parse("11111111111111111111111111111111");

            questionDeletedEvent = CreateQuestionDeletedEvent(questionId);

            QuestionnaireDocument questionnaire = CreateQuestionnaireDocument(children: new []
            {
                singleGroup = CreateGroup(children: new[]
                {
                    CreateQuestion(questionId: questionId, title: "Question 1"),
                    secondQuestion = CreateQuestion(questionId: questionId, title: "Question 2"),
                    thirdQuestion = CreateQuestion(questionId: questionId, title: "Question 3"),
                    forthQuestion = CreateQuestion(questionId: questionId, title: "Question 4"),
                }),
            });

            var documentStorage = Mock.Of<IDenormalizerStorage<QuestionnaireDocument>>(storage
                => storage.GetById(it.IsAny<Guid>()) == questionnaire);

            denormalizer = CreateQuestionnaireDenormalizer(documentStorage: documentStorage);
        };

        Because of = () =>
            denormalizer.Handle(questionDeletedEvent);

        It should_be_only_3_questions_left = () =>
            singleGroup.Children.Count.ShouldEqual(3);

        It should_result_in_only_second_third_and_forth_questions_left = () =>
            singleGroup.Children.ShouldContainOnly(secondQuestion, thirdQuestion, forthQuestion);

        private static Group singleGroup;
        private static AbstractQuestion secondQuestion;
        private static AbstractQuestion thirdQuestion;
        private static AbstractQuestion forthQuestion;
        private static QuestionnaireDenormalizer denormalizer;
        private static IPublishedEvent<QuestionDeleted> questionDeletedEvent;
    }
}