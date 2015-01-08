using System;
using Machine.Specifications;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireTests.CloneStaticTextTests
{
    public class when_cloning_StaticText : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            responsibleId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            sourceStaticTextId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            targetId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            sourceParentId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            sourceText = "source text";

            questionnaire = CreateQuestionnaireWithOneGroup(responsibleId, groupId: sourceParentId);
            questionnaire.Apply(Create.Event.StaticTextAdded(parentId: sourceParentId, 
                                                             responsibleId: responsibleId, 
                                                             publicKey: sourceStaticTextId,
                                                             text: sourceText));

            eventContext = new EventContext();
        };

        Because of = () => questionnaire.CloneStaticText(targetId, sourceStaticTextId, responsibleId);

        It should_produce_StaticTextCloned_event_with_source_properties = () => eventContext.ShouldContainEvent<StaticTextCloned>(e => 
            e.EntityId == targetId && 
            e.ParentId == sourceParentId &&
            e.SourceEntityId == sourceStaticTextId &&
            e.Text == sourceText &&
            e.ResponsibleId == responsibleId &&
            e.TargetIndex == 1);

        static Guid responsibleId;
        static Questionnaire questionnaire;
        static EventContext eventContext;
        static Guid sourceStaticTextId;
        static Guid targetId;
        static Guid sourceParentId;
        static string sourceText;
    }
}

