using System;
using FluentAssertions;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.StaticText;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.MoveStaticTextHandlerTests
{
    internal class when_moving_static_text_and_target_entity_does_not_exists : QuestionnaireTestsContext
    {
        [NUnit.Framework.Test] public void should_throw_QuestionnaireException () {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
            questionnaire.AddStaticTextAndMoveIfNeeded(new AddStaticText(questionnaire.Id, entityId, "title", responsibleId, chapterId));
            exception = Assert.Throws<QuestionnaireException>(() =>
                questionnaire.MoveStaticText(entityId: entityId, responsibleId: responsibleId, targetEntityId: notExistingEntityId, targetIndex: 0));

            exception.Message.ToLower().ToSeparateWords().Should().Contain(new[] { "sub", "section", "can't", "found" });
        }
        
        private static Questionnaire questionnaire;
        private static Exception exception;
        private static Guid entityId = Guid.Parse("11111111111111111111111111111112");
        private static Guid notExistingEntityId = Guid.Parse("22222222222222222222222222222222");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
    }
}
