using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_moving_roster_nested_group_with_numeric_question_with_title_which_contains_roster_title_as_substitution_to_group : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            questionId = Guid.Parse("11111111111111111111111111111111");
            rosterId = Guid.Parse("21111111111111111111111111111111");
            nestedRosterId = Guid.Parse("31111111111111111111111111111111");

            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
            questionnaire.AddGroup(rosterId,chapterId, responsibleId: responsibleId);
            questionnaire.MarkGroupAsRoster(new GroupBecameARoster(Guid.NewGuid(), rosterId));

            questionnaire.AddGroup(nestedRosterId,  rosterId, responsibleId: responsibleId);
            questionnaire.AddNumericQuestion(
                questionId,
                nestedRosterId,
                responsibleId,
                title : questionTitle,
                variableName : "var");
            eventContext = new EventContext();
        };

        Because of = () => exception = Catch.Exception(() => questionnaire.MoveGroup(nestedRosterId, chapterId, 1, responsibleId));

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        It should_throw_exception_with_message_containting__questions____substitution__and_variable_name_of_question_with_rostertitle_in_substitution = () =>
          new[] { "questions", "substitution", "var" }.ShouldEachConformTo(
          keyword => exception.Message.ToLower().Contains(keyword));


        private static EventContext eventContext;
        private static Questionnaire questionnaire;
        private static Guid questionId;
        private static Guid chapterId;
        private static Guid rosterId;
        private static Guid nestedRosterId;
        private static Guid responsibleId;
        private static Exception exception;
        private static string questionTitle = "title %rostertitle%";
    }
}
