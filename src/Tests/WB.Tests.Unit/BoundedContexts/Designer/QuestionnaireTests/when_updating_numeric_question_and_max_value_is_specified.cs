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

            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.Apply(new NewQuestionAdded { PublicKey = questionId, QuestionType = QuestionType.Text });

            eventContext = new EventContext();
        };

        Because of = () =>
            questionnaire.UpdateNumericQuestion(questionId, "title",
                "var1",null, false, false, QuestionScope.Interviewer, null, null, null, null,
                responsibleId, true, null);

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

        private static EventContext eventContext;
        private static Questionnaire questionnaire;
        private static Guid questionId;
        private static Guid chapterId;
        private static Guid responsibleId;
    }
}