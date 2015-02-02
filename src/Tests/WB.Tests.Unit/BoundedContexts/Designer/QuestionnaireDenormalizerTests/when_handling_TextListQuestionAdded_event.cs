using System;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities;
using Main.Core.Entities.SubEntities;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Document;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireDenormalizerTests
{
    internal class when_handling_TextListQuestionAdded_event : QuestionnaireDenormalizerTestsContext
    {
        Establish context = () =>
        {
            var parentGroupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            @event = CreateTextListQuestionAddedEvent(questionId: questionId, parentGroupId: parentGroupId);

            var questionnaireDocument = CreateQuestionnaireDocument(new[]
            {
                CreateGroup(groupId: parentGroupId)
            });

            var documentStorage = Mock.Of<IReadSideKeyValueStorage<QuestionnaireDocument>>(writer => writer.GetById(Moq.It.IsAny<string>()) == questionnaireDocument);

            var questionFactory = new Mock<IQuestionnaireEntityFactory>();

            questionFactory.Setup(x => x.CreateQuestion(Moq.It.IsAny<QuestionData>()))
                .Callback<QuestionData>(d => questionData = d);

            denormalizer = CreateQuestionnaireDenormalizer(documentStorage: documentStorage, questionnaireEntityFactory: questionFactory.Object);
        };

        Because of = () =>
            denormalizer.Handle(@event);

        It should_set_null_as_default_value_for__ValidationExpression__field = () =>
           questionData.ValidationExpression.ShouldBeNull();

        It should_set_null_as_default_value_for__ValidationMessage__field = () =>
            questionData.ValidationMessage.ShouldBeNull();

        It should_set_Interviewer_as_default_value_for__QuestionScope__field = () =>
            questionData.QuestionScope.ShouldEqual(QuestionScope.Interviewer);

        It should_set_false_as_default_value_for__Featured__field = () =>
            questionData.Featured.ShouldBeFalse();

        It should_set_TextList_as_default_value_for__QuestionType__field = () =>
            questionData.QuestionType.ShouldEqual(QuestionType.TextList);

        private static QuestionData questionData;
        private static QuestionnaireDenormalizer denormalizer;
        private static IPublishedEvent<TextListQuestionAdded> @event;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
    }
}