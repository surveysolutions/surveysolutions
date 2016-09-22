using System;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities;
using Main.Core.Entities.SubEntities;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireDenormalizerTests
{
    internal class when_handling_TextListQuestionChanged_event : QuestionnaireDenormalizerTestsContext
    {
        Establish context = () =>
        {
            var parentGroupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            @event = CreateTextListQuestionChangedEvent(questionId: questionId);

            var questionnaireDocument = CreateQuestionnaireDocument(new[]
            {
                CreateGroup(groupId: parentGroupId, children: new []
                {
                    CreateTextListQuestion(questionId: questionId)
                })
            });

            var questionFactory = new Mock<IQuestionnaireEntityFactory>();

            var updatedQuestion = CreateTextListQuestion(questionId: questionId);

            questionFactory.Setup(x => x.CreateQuestion(Moq.It.IsAny<QuestionData>()))
                .Callback<QuestionData>(d => questionData = d)
                .Returns(() => updatedQuestion);

            denormalizer = CreateQuestionnaireDenormalizer(questionnaire: questionnaireDocument, questionnaireEntityFactory: questionFactory.Object);
        };

        Because of = () =>
            denormalizer.UpdateTextListQuestion(@event);

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
        private static Questionnaire denormalizer;
        private static TextListQuestionChanged @event;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
    }
}