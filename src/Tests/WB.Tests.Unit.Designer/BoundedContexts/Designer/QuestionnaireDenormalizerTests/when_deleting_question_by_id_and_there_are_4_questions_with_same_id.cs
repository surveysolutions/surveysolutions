using System;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Aggregates;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireDenormalizerTests
{
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
                    CreateTextQuestion(questionId: questionId, title: "Question 1"),
                    secondQuestion = CreateTextQuestion(questionId: questionId, title: "Question 2"),
                    thirdQuestion = CreateTextQuestion(questionId: questionId, title: "Question 3"),
                    forthQuestion = CreateTextQuestion(questionId: questionId, title: "Question 4"),
                }),
            });

            denormalizer = CreateQuestionnaireDenormalizer(questionnaire: questionnaire);
        };

        Because of = () =>
            denormalizer.DeleteQuestion(questionDeletedEvent.Payload);

        It should_be_only_3_questions_left = () =>
            singleGroup.Children.Count.ShouldEqual(3);

        It should_result_in_only_second_third_and_forth_questions_left = () =>
            singleGroup.Children.ShouldContainOnly(secondQuestion, thirdQuestion, forthQuestion);

        private static Group singleGroup;
        private static AbstractQuestion secondQuestion;
        private static AbstractQuestion thirdQuestion;
        private static AbstractQuestion forthQuestion;
        private static Questionnaire denormalizer;
        private static IPublishedEvent<QuestionDeleted> questionDeletedEvent;
    }
}