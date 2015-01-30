using System;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Document;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireDenormalizerTests
{
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
                    existingQuestion = CreateTextQuestion(questionId: questionId, title: "Existing Question"),
                }),
            });

            var documentStorage = Mock.Of<IReadSideKeyValueStorage<QuestionnaireDocument>>(storage
                => storage.GetById(it.IsAny<string>()) == questionnaire);

            var textQuestion = CreateTextQuestion(questionId: questionId, title: addedQuestionTitle);

            var questionFactory = Mock.Of<IQuestionnaireEntityFactory>(x =>
                x.CreateQuestion(Moq.It.IsAny<QuestionData>()) == textQuestion
            );

            denormalizer = CreateQuestionnaireDenormalizer(documentStorage: documentStorage, questionnaireEntityFactory: questionFactory);
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