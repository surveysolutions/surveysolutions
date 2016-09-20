using System;
using System.Linq;
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
using WB.Core.SharedKernels.QuestionnaireEntities;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireDenormalizerTests
{
    internal class when_handling_TextListQuestionCloned_event : QuestionnaireDenormalizerTestsContext
    {
        Establish context = () =>
        {
            var parentGroupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            @event = CreateTextListQuestionClonedEvent(questionId: questionId, sourceQuestionId: sourceQuestionId);
            @event.Payload.ValidationConditions.Add(new ValidationCondition(validation, validationMessage));
            @event.Payload.ValidationConditions.Add(new ValidationCondition(validation1, validationMessage1));
            var questionnaireDocument = CreateQuestionnaireDocument(new[]
            {
                CreateGroup(groupId: parentGroupId, children: new []
                {
                    CreateTextQuestion(questionId: questionId)
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
            denormalizer.CloneTextListQuestion(@event.Payload);

        It should_set_validation_value_for__ValidationExpression__field = () =>
            questionData.ValidationConditions.First().Expression.ShouldEqual(validation);

        It should_set_validationMessage_value_for__ValidationMessage__field = () =>
            questionData.ValidationConditions.First().Message.ShouldEqual(validationMessage);

        It should_set_validation_value_for_second_ValidationExpression__field = () =>
           questionData.ValidationConditions.Second().Expression.ShouldEqual(validation1);

        It should_set_validationMessage_value_for_second_ValidationMessage__field = () =>
            questionData.ValidationConditions.Second().Message.ShouldEqual(validationMessage1);

        It should_set_Interviewer_as_default_value_for__QuestionScope__field = () =>
            questionData.QuestionScope.ShouldEqual(QuestionScope.Interviewer);

        It should_set_false_as_default_value_for__Featured__field = () =>
            questionData.Featured.ShouldBeFalse();

        It should_set_TextList_as_default_value_for__QuestionType__field = () =>
            questionData.QuestionType.ShouldEqual(QuestionType.TextList);

        private static QuestionData questionData;
        private static Questionnaire denormalizer;
        private static IPublishedEvent<TextListQuestionCloned> @event;
        private static Guid sourceQuestionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid questionId = Guid.Parse("22222222222222222222222222222222");
        private static string validation = "validation";
        private static string validation1 = "validation1";
        private static string validationMessage = "validationMessage";
        private static string validationMessage1 = "validationMessage1";
    }
}