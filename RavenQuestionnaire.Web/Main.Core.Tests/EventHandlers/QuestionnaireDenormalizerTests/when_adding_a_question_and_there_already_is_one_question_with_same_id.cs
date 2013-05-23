namespace Main.Core.Tests.Domain.QuestionnaireDenormalizerTests
{
    using System;

    using Machine.Specifications;

    using Main.Core.Documents;
    using Main.Core.Entities.SubEntities;
    using Main.Core.EventHandlers;
    using Main.Core.Events.Questionnaire;
    using Main.DenormalizerStorage;

    using Moq;

    using Ncqrs.Eventing.ServiceModel.Bus;

    using It = Machine.Specifications.It;
    using it = Moq.It;

    internal class when_adding_a_question_and_there_already_is_one_question_with_same_id : QuestionnaireDenormalizerTestsContext
    {
        Establish context = () =>
        {
            var questionId = Guid.Parse("11111111111111111111111111111111");
            var singleGroupId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
            addedQuestionTitle = "Added Question";

            questionAddedEvent = CreateNewQuestionAddedEvent(questionId: questionId, groupId: singleGroupId, title: addedQuestionTitle);

            QuestionnaireDocument questionnaire = CreateQuestionnaireDocument(children: new[]
            {
                singleGroup = CreateGroup(groupId: singleGroupId, children: new[]
                {
                    existingQuestion = CreateQuestion(questionId: questionId, title: "Existing Question"),
                }),
            });

            var documentStorage = Mock.Of<IDenormalizerStorage<QuestionnaireDocument>>(storage
                => storage.GetById(it.IsAny<Guid>()) == questionnaire);

            denormalizer = CreateQuestionnaireDenormalizer(documentStorage: documentStorage);
        };

        Because of = () =>
            denormalizer.Handle(questionAddedEvent);

        It should_be_2_questions_in_total = () =>
            singleGroup.Children.Count.ShouldEqual(2);

        It should_be_existing_question_in_questionnaire = () =>
            singleGroup.Children.ShouldContain(existingQuestion);

        It should_be_added_question_in_questionnaire = () =>
            singleGroup.Children.ShouldContain(question => ((AbstractQuestion)question).QuestionText == addedQuestionTitle);

        private static QuestionnaireDenormalizer denormalizer;
        private static IPublishedEvent<NewQuestionAdded> questionAddedEvent;
        private static Group singleGroup;
        private static AbstractQuestion existingQuestion;
        private static string addedQuestionTitle;
    }
}