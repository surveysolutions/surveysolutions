using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.StaticText;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.AddStaticTextHandlerTests
{
    internal class when_adding_static_text_to_chapter : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(new NewGroupAdded { PublicKey = chapterId });
        };

        Because of = () =>
                questionnaire.AddStaticTextAndMoveIfNeeded(
                    new AddStaticText(questionnaire.EventSourceId, entityId, text, responsibleId, chapterId, index));


        It should_raise_StaticTextAdded_event_with_EntityId_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IStaticText>(entityId).ShouldNotBeNull();

        It should_raise_StaticTextAdded_event_with_ParentId_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IStaticText>(entityId).GetParent().PublicKey.ShouldEqual(chapterId);

        It should_raise_StaticTextAdded_event_with_Text_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IStaticText>(entityId).Text.ShouldEqual(text);


        private static Questionnaire questionnaire;
        private static Guid entityId = Guid.Parse("11111111111111111111111111111111");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static string text = "some text";
        private static int index = 5;
    }
}