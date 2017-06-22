using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;


namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_moving_group_to_negative_position : QuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            groupId = Guid.Parse("21111111111111111111111111111111");

            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
            questionnaire.AddGroup(groupId, chapterId, responsibleId: responsibleId);


            BecauseOf();
        }

        private void BecauseOf() => exception = Catch.Exception(() => questionnaire.MoveGroup(chapterId, chapterId, -1, responsibleId));


        [NUnit.Framework.Test] public void should_throw_QuestionnaireException () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        [NUnit.Framework.Test] public void should_throw_exception_with_message_containting__move____group____position____acceptable__ () =>
          new[] { "move", "section", "position", "acceptable" }.ShouldEachConformTo(
          keyword => exception.Message.ToLower().Contains(keyword));


        private static Questionnaire questionnaire;
        private static Guid chapterId;
        private static Guid groupId;
        private static Guid responsibleId;
        private static Exception exception;
    }
}
