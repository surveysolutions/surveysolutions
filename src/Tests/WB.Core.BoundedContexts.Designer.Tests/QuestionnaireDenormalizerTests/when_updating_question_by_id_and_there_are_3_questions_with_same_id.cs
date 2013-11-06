using System;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Document;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireDenormalizerTests
{
    using Main.Core.Entities;

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
                mainGroup = CreateGroup(children: new[]
                {
                    CreateQuestion(questionId: questionId, title: initialQuestionTitle),
                    CreateQuestion(questionId: questionId, title: initialQuestionTitle),
                    CreateQuestion(questionId: questionId, title: initialQuestionTitle),
                }),
            });

            var documentStorage = Mock.Of<IReadSideRepositoryWriter<QuestionnaireDocument>>(storage
                => storage.GetById(it.IsAny<Guid>()) == questionnaire);

            var questionFactory = Mock.Of<IQuestionFactory>();

            Mock.Get(questionFactory)
                .Setup(factory => factory.CreateQuestion(it.IsAny<QuestionData>()))
                .Returns((QuestionData question) => new QuestionFactory().CreateQuestion(question));

            denormalizer = CreateQuestionnaireDenormalizer(documentStorage: documentStorage, questionFactory: questionFactory);
        };

        Because of = () =>
            denormalizer.Handle(questionUpdatedEvent);

        It should_update_first_question = () =>
            ((IQuestion)mainGroup.Children[0]).QuestionText.ShouldEqual(updatedQuestionTitle);

        It should_not_update_second_question = () =>
            ((IQuestion)mainGroup.Children[1]).QuestionText.ShouldEqual(initialQuestionTitle);

        It should_not_update_third_question = () =>
            ((IQuestion)mainGroup.Children[2]).QuestionText.ShouldEqual(initialQuestionTitle);

        private static QuestionnaireDenormalizer denormalizer;
        private static IPublishedEvent<QuestionChanged> questionUpdatedEvent;
        private static string initialQuestionTitle;
        private static string updatedQuestionTitle;
        private static Group mainGroup;
    }
}