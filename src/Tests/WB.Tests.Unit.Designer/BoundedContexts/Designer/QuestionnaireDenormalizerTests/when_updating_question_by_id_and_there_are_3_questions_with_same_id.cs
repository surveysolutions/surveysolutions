using System;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities;
using Main.Core.Entities.SubEntities;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using Group = Main.Core.Entities.SubEntities.Group;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireDenormalizerTests
{
    internal class when_updating_question_by_id_and_there_are_3_questions_with_same_id : QuestionnaireDenormalizerTestsContext
    {
        Establish context = () =>
        {
            var questionId = Guid.Parse("11111111111111111111111111111111");

            initialQuestionTitle = "Initial Title";
            updatedQuestionTitle = "Updated Title";

            Guid groupId  = Guid.NewGuid();

            questionUpdatedEvent = CreateQuestionChangedEvent(questionId: questionId, targetGroupId: groupId, title: updatedQuestionTitle);

            QuestionnaireDocument questionnaire = CreateQuestionnaireDocument(children: new[] 
            {
                mainGroup = CreateGroup(groupId: groupId, children: new[]
                {
                    CreateTextQuestion(questionId: questionId, title: initialQuestionTitle),
                    CreateTextQuestion(questionId: questionId, title: initialQuestionTitle),
                    CreateTextQuestion(questionId: questionId, title: initialQuestionTitle),
                }),
            });

            var questionFactory = Mock.Of<IQuestionnaireEntityFactory>();

            Mock.Get(questionFactory)
                .Setup(factory => factory.CreateQuestion(it.IsAny<QuestionData>()))
                .Returns((QuestionData question) => new QuestionnaireEntityFactory().CreateQuestion(question));

            denormalizer = CreateQuestionnaireDenormalizer(questionnaire: questionnaire, questionnaireEntityFactory: questionFactory);
        };

        Because of = () =>
            denormalizer.UpdateQuestion(questionUpdatedEvent);

        It should_update_first_question = () =>
            ((IQuestion)mainGroup.Children[0]).QuestionText.ShouldEqual(updatedQuestionTitle);

        It should_not_update_second_question = () =>
            ((IQuestion)mainGroup.Children[1]).QuestionText.ShouldEqual(initialQuestionTitle);

        It should_not_update_third_question = () =>
            ((IQuestion)mainGroup.Children[2]).QuestionText.ShouldEqual(initialQuestionTitle);

        private static Questionnaire denormalizer;
        private static QuestionChanged questionUpdatedEvent;
        private static string initialQuestionTitle;
        private static string updatedQuestionTitle;
        private static Group mainGroup;
    }
}