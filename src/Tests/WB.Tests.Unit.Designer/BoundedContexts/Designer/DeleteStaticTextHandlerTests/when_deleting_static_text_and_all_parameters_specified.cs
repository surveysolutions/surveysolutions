using System;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.StaticText;

using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.DeleteStaticTextHandlerTests
{
    internal class when_deleting_static_text_and_all_parameters_specified : QuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
            questionnaire.AddStaticTextAndMoveIfNeeded(new AddStaticText(questionnaire.Id, entityId, "title", responsibleId, chapterId));
            BecauseOf();
        }

        private void BecauseOf() =>            
                questionnaire.DeleteStaticText(entityId: entityId, responsibleId: responsibleId);


        [NUnit.Framework.Test] public void should_dont_contains_StaticText () =>
            questionnaire.QuestionnaireDocument.Find<IStaticText>(entityId).Should().BeNull();

        
        private static Questionnaire questionnaire;
        private static Guid entityId = Guid.Parse("11111111111111111111111111111112");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
    }
}
