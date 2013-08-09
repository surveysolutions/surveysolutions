using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.AbstractFactories;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Document;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireDenormalizerTests
{
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

            var documentStorage = Mock.Of<IReadSideRepositoryWriter<QuestionnaireDocument>>(storage
                => storage.GetById(it.IsAny<Guid>()) == questionnaire);

            var questionFactory = Mock.Of<ICompleteQuestionFactory>();
            Mock.Get(questionFactory)
                .Setup(factory => factory.CreateQuestionFromExistingUsingSpecifiedData(
                    it.IsAny<IQuestion>(), it.IsAny<QuestionType>(), it.IsAny<QuestionScope>(),
                    it.IsAny<string>(), it.IsAny<string>(), it.IsAny<string>(), it.IsAny<string>(),
                    it.IsAny<string>(), it.IsAny<Order>(), it.IsAny<bool>(), it.IsAny<bool>(), it.IsAny<bool>(),
                    it.IsAny<string>(), it.IsAny<List<Guid>>(), it.IsAny<int>(), it.IsAny<Answer[]>()))
                .Returns(delegate(IQuestion question, QuestionType questionType, QuestionScope questionScope, string questionText, string stataExportCaption, string conditionExpression, string validationExpression, string validationMessage, Order answerOrder, bool featured, bool mandatory, bool capital, string instructions, List<Guid> triggers, int maxValue, Answer[] answers)
                {
                    question.QuestionText = questionText;
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