using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;


namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_moving_group_inside_the_same_parent_to_position_which_is_bigger_then_parent_children_count : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            groupId = Guid.Parse("21111111111111111111111111111111");

            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
            questionnaire.AddGroup(groupId, chapterId, responsibleId: responsibleId);

            eventContext = new EventContext();
        };

        Because of = () => exception = Catch.Exception(() => questionnaire.MoveGroup(groupId, chapterId, 1, responsibleId));

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        It should_throw_exception_with_message_containting__move____group____position____acceptable__ = () =>
          new[] { "move", "section", "position", "acceptable" }.ShouldEachConformTo(
          keyword => exception.Message.ToLower().Contains(keyword));


        private static EventContext eventContext;
        private static Questionnaire questionnaire;
        private static Guid chapterId;
        private static Guid groupId;
        private static Guid responsibleId;
        private static Exception exception;
    }
}
