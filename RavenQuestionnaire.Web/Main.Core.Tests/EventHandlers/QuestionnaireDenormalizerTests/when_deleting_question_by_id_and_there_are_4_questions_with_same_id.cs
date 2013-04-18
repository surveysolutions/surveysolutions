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

            QuestionnaireDocument questionnaire = CreateQuestionnaireDocument();

            questionnaire.Children.Add
            (
                group = CreateGroup()
            );

            group.Children.AddRange(new[]
            {
                firstQuestion = CreateQuestion(questionId: questionId, title: "Question 1"),
                secondQuestion = CreateQuestion(questionId: questionId, title: "Question 2"),
                thirdQuestion = CreateQuestion(questionId: questionId, title: "Question 3"),
                forthQuestion = CreateQuestion(questionId: questionId, title: "Question 4"),
            });

            var documentStorage = Mock.Of<IDenormalizerStorage<QuestionnaireDocument>>(storage
                => storage.GetByGuid(it.IsAny<Guid>()) == questionnaire);

            denormalizer = CreateQuestionnaireDenormalizer(documentStorage: documentStorage);
        };

        Because of = () =>
            denormalizer.Handle(questionDeletedEvent);

        It should_be_only_3_questions_left = () =>
            group.Children.Count.ShouldEqual(3);

        It should_remove_first_question = () =>
            group.Children.ShouldNotContain(firstQuestion);

        It should_not_remove_second_question = () =>
            group.Children.ShouldContain(secondQuestion);

        It should_not_remove_thrid_question = () =>
            group.Children.ShouldContain(thirdQuestion);

        It should_not_remove_forth_question = () =>
            group.Children.ShouldContain(forthQuestion);

        private static Group group;
        private static AbstractQuestion firstQuestion;
        private static AbstractQuestion secondQuestion;
        private static AbstractQuestion thirdQuestion;
        private static AbstractQuestion forthQuestion;
        private static QuestionnaireDenormalizer denormalizer;
        private static IPublishedEvent<QuestionDeleted> questionDeletedEvent;
    }
}