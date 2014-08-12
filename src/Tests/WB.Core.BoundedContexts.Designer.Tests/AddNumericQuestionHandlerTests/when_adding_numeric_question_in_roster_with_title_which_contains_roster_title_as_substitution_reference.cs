using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests;

namespace WB.Core.BoundedContexts.Designer.Tests.AddNumericQuestionHandlerTests
{
    internal class when_adding_numeric_question_in_roster_with_title_which_contains_roster_title_as_substitution_reference : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            questionId = Guid.Parse("11111111111111111111111111111111");
            rosterId = Guid.Parse("21111111111111111111111111111111");
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.Apply(new NewGroupAdded { PublicKey = rosterId, ParentGroupPublicKey = chapterId });
            questionnaire.Apply(new GroupBecameARoster(Guid.NewGuid(), rosterId));
            eventContext = new EventContext();
        };

        Because of = () => questionnaire.AddNumericQuestion(questionId, rosterId, questionTitle, "var",null, false, false, QuestionScope.Interviewer, null, null, null, null,
                responsibleId: responsibleId, isInteger: false, countOfDecimalPlaces: null,
                maxValue: null);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_NumericQuestionAdded_event = () =>
        eventContext.ShouldContainEvent<NumericQuestionAdded>();

        It should_raise_NumericQuestionAdded_event_with_PublicKey_equal_to_question_id = () =>
            eventContext.GetSingleEvent<NumericQuestionAdded>()
                .PublicKey.ShouldEqual(questionId);

        It should_raise_NumericQuestionAdded_event_with_MaxAllowedValue_equal_null = () =>
            eventContext.GetSingleEvent<NumericQuestionAdded>()
                .QuestionText.ShouldEqual(questionTitle);

        private static EventContext eventContext;
        private static Questionnaire questionnaire;
        private static Guid questionId;
        private static Guid chapterId;
        private static Guid rosterId;
        private static Guid responsibleId;

        private static string questionTitle = "title %rostertitle%";
    }
}
