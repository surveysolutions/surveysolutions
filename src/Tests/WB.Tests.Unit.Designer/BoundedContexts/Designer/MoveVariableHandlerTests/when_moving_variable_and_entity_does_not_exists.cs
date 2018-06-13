using System;
using FluentAssertions;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;

using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.MoveVariableHandlerTests
{
    internal class when_moving_variable_and_entity_does_not_exists : QuestionnaireTestsContext
    {
        [NUnit.Framework.Test] public void should_throw_QuestionnaireException () {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
            questionnaire.AddVariable(entityId, parentId : chapterId, responsibleId:responsibleId);

            exception = Assert.Throws<QuestionnaireException>(() =>
                questionnaire.MoveStaticText(entityId: notExistingEntityId, responsibleId: responsibleId, targetEntityId: chapterId, targetIndex: 0));

            exception.Message.ToLower().ToSeparateWords().Should().Contain(new[] { "item", "can't", "found" });
        }

        private static Questionnaire questionnaire;
        private static Exception exception;
        private static Guid entityId = Guid.Parse("11111111111111111111111111111111");
        private static Guid notExistingEntityId = Guid.Parse("22222222222222222222222222222222");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
    }
}
