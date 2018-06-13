using System;
using FluentAssertions;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;


namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_moving_group_to_position_which_is_bigger_then_group_children_count : QuestionnaireTestsContext
    {
        [NUnit.Framework.Test] public void should_throw_QuestionnaireException () {
            responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            groupId = Guid.Parse("21111111111111111111111111111111");
            parentGroupId = Guid.Parse("33333333333333333333333333333333");

            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
            questionnaire.AddGroup(parentGroupId,  chapterId, responsibleId: responsibleId);
            questionnaire.AddGroup(groupId, parentGroupId, responsibleId: responsibleId);

            var exception = Assert.Throws<QuestionnaireException>(() => questionnaire.MoveGroup(groupId, chapterId, 2, responsibleId));

            exception.Message.ToLower().ToSeparateWords().Should().Contain(new[] { "move", "section", "position", "acceptable" });
        }

        private static Questionnaire questionnaire;
        private static Guid chapterId;
        private static Guid parentGroupId;
        private static Guid groupId;
        private static Guid responsibleId;
    }
}
