using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_updating_numeric_question_and_max_value_is_not_specified : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            questionId = Guid.Parse("11111111111111111111111111111111");

            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(new NewGroupAdded { PublicKey = chapterId });
            
            questionnaire.AddQuestion(Create.Event.NewQuestionAdded (publicKey : questionId, groupPublicKey : chapterId, questionType : QuestionType.Text ));

            eventContext = new EventContext();
        };

        Because of = () =>
            questionnaire.UpdateNumericQuestion(questionId, "title",
                "var1",null, false, QuestionScope.Interviewer, null, false, null, properties: Create.QuestionProperties(), responsibleId: responsibleId, isInteger: false, countOfDecimalPlaces: null, validationConditions: new List<ValidationCondition>());

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