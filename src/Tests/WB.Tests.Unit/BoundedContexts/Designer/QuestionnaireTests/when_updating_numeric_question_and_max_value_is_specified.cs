using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireTests
{
    internal class when_updating_numeric_question_and_max_value_is_specified : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            questionId = Guid.Parse("11111111111111111111111111111111");
            maxValue = 42;

            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.Apply(new NewQuestionAdded { PublicKey = questionId, QuestionType = QuestionType.Text });

            eventContext = new EventContext();
        };

        Because of = () =>
            questionnaire.UpdateNumericQuestion(questionId, "title",
                "var1",null, false, false, QuestionScope.Interviewer, null, null, null, null,
                maxValue, responsibleId, true, null);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_NumericQuestionChanged_event = () =>
            eventContext.ShouldContainEvent<NumericQuestionChanged>();

        It should_raise_NumericQuestionChanged_event_with_PublicKey_equal_to_question_id = () =>
            eventContext.GetSingleEvent<NumericQuestionChanged>()
                .PublicKey.ShouldEqual(questionId);

        It should_raise_NumericQuestionChanged_event_with_MaxAllowedValue_equal_to_specified_max_value = () =>
            eventContext.GetSingleEvent<NumericQuestionChanged>()
                .MaxAllowedValue.ShouldEqual(maxValue);

        private static EventContext eventContext;
        private static int maxValue;
        private static Questionnaire questionnaire;
        private static Guid questionId;
        private static Guid chapterId;
        private static Guid responsibleId;
    }
}