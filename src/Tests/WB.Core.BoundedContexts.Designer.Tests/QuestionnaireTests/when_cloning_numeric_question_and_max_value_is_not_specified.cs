using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests
{
    internal class when_cloning_numeric_question_and_max_value_is_not_specified : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            sourceQuestionId = Guid.Parse("44444444444444444444444444444444");
            questionId = Guid.Parse("11111111111111111111111111111111");

            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.Apply(new NewQuestionAdded { PublicKey = sourceQuestionId, QuestionType = QuestionType.Text });

            eventContext = new EventContext();
        };

        Because of = () =>
            questionnaire.CloneNumericQuestion(questionId, chapterId, "title",
                false, "var1", false, false, false, QuestionScope.Interviewer, null, null, null, null,
                sourceQuestionId, 0, responsibleId, triggeredGroupIds: new Guid[] { }, isInteger: false, countOfDecimalPlaces: null,
                maxValue: null);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_NumericQuestionCloned_event = () =>
            eventContext.ShouldContainEvent<NumericQuestionCloned>();

        It should_raise_NumericQuestionCloned_event_with_PublicKey_equal_to_question_id = () =>
            eventContext.GetSingleEvent<NumericQuestionCloned>()
                .PublicKey.ShouldEqual(questionId);

        It should_raise_NumericQuestionCloned_event_with_MaxAllowedValue_equal_null = () =>
            eventContext.GetSingleEvent<NumericQuestionCloned>()
                .MaxAllowedValue.ShouldBeNull();

        private static EventContext eventContext;
        private static Questionnaire questionnaire;
        private static Guid questionId;
        private static Guid chapterId;
        private static Guid responsibleId;
        private static Guid sourceQuestionId;
    }
}