using System;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Document;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireDenormalizerTests
{
    internal class when_moving_question_to_another_group_by_id_and_there_are_4_questions_with_same_id_in_initial_group : QuestionnaireDenormalizerTestsContext
    {
        Establish context = () =>
        {
            var questionId = Guid.Parse("11111111111111111111111111111111");
            var anotherGroupId = Guid.Parse("dadadadadadadadadadadadadadadada");

            questionMovedEvent = CreateQuestionnaireItemMovedEvent(itemId: questionId, targetGroupId: anotherGroupId);

            QuestionnaireDocument questionnaire = CreateQuestionnaireDocument(children: new[] 
            {
                initialGroup = CreateGroup(children: new[]
                {
                    firstQuestion = CreateTextQuestion(questionId: questionId, title: "Question 1"),
                    secondQuestion = CreateTextQuestion(questionId: questionId, title: "Question 2"),
                    thirdQuestion = CreateTextQuestion(questionId: questionId, title: "Question 3"),
                    forthQuestion = CreateTextQuestion(questionId: questionId, title: "Question 4"),
                }),

                anotherGroup = CreateGroup(groupId: anotherGroupId),
            });

            var documentStorage = Mock.Of<IReadSideKeyValueStorage<QuestionnaireDocument>>(storage
                => storage.GetById(it.IsAny<string>()) == questionnaire);

            denormalizer = CreateQuestionnaireDenormalizer(documentStorage: documentStorage);
        };

        Because of = () =>
            denormalizer.Handle(questionMovedEvent);

        It should_move_first_question_to_another_group = () =>
            anotherGroup.Children.ShouldContainOnly(firstQuestion);

        It should_leave_second_third_and_forth_questions_in_initial_group = () =>
            initialGroup.Children.ShouldContainOnly(secondQuestion, thirdQuestion, forthQuestion);

        private static Group anotherGroup;
        private static Group initialGroup;
        private static AbstractQuestion firstQuestion;
        private static AbstractQuestion secondQuestion;
        private static AbstractQuestion thirdQuestion;
        private static AbstractQuestion forthQuestion;
        private static QuestionnaireDenormalizer denormalizer;
        private static IPublishedEvent<QuestionnaireItemMoved> questionMovedEvent;
    }
}