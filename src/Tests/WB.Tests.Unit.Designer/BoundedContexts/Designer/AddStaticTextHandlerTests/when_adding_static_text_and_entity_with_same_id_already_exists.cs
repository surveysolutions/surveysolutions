using System;
using FluentAssertions;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.StaticText;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.AddStaticTextHandlerTests
{
    internal class when_adding_static_text_and_entity_with_same_id_already_exists : QuestionnaireTestsContext
    {
        [NUnit.Framework.Test] public void should_throw_QuestionnaireException () {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
            questionnaire.AddDefaultTypeQuestionAdnMoveIfNeeded(Create.Command.AddDefaultTypeQuestion(questionnaire.Id, entityId, "title", responsibleId, chapterId));

            exception = Assert.Throws<QuestionnaireException>(() =>
                            questionnaire.AddStaticTextAndMoveIfNeeded(
                                new AddStaticText(questionnaire.Id, entityId, "title", responsibleId, chapterId)));

            exception.Message.ToLower().ToSeparateWords().Should().Contain(new[] {"id", "exists"});
        }

        private static Questionnaire questionnaire;
        private static Exception exception;
        private static Guid entityId = Guid.Parse("11111111111111111111111111111111");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
    }
}
