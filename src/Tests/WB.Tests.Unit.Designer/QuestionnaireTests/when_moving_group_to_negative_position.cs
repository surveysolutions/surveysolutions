using System;
using FluentAssertions;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;


namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_moving_group_to_negative_position : QuestionnaireTestsContext
    {
        [NUnit.Framework.Test] public void should_throw_QuestionnaireException () {
            responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            groupId = Guid.Parse("21111111111111111111111111111111");

            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
            questionnaire.AddGroup(groupId, chapterId, responsibleId: responsibleId);

            BecauseOf();
            exception.Message.ToLower().ToSeparateWords().Should().Contain(new[] { "move", "section", "position", "acceptable" });
        }

        private void BecauseOf() => exception = Assert.Throws<QuestionnaireException>(() => questionnaire.MoveGroup(chapterId, chapterId, -1, responsibleId));


        private static Questionnaire questionnaire;
        private static Guid chapterId;
        private static Guid groupId;
        private static Guid responsibleId;
        private static Exception exception;
    }
}
