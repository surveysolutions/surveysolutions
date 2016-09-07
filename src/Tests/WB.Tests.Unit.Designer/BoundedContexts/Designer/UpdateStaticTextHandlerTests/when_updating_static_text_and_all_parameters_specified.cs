using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.StaticText;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.UpdateStaticTextHandlerTests
{
    internal class when_updating_static_text_and_all_parameters_specified : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.AddStaticText(Create.Event.StaticTextAdded(entityId : entityId, parentId : chapterId ));

            eventContext = new EventContext();

            command = Create.Command.UpdateStaticText(
                questionnaire.EventSourceId,
                entityId: entityId,
                text: text,
                attachmentName: "",
                enablementCondition : enablementCondition,
                responsibleId: responsibleId,
                hideIfDisabled: true,
                validationConditions: new List<ValidationCondition>() { new ValidationCondition(validationCondition, validationMessage) }
                );
        };

        Because of = () =>            
                questionnaire.UpdateStaticText(command);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_StaticTextUpdated_event = () =>
            eventContext.ShouldContainEvent<StaticTextUpdated>();

        It should_raise_StaticTextUpdated_event_with_EntityId_specified = () =>
            eventContext.GetSingleEvent<StaticTextUpdated>().EntityId.ShouldEqual(entityId);

        It should_raise_StaticTextUpdated_event_with_Text_specified = () =>
            eventContext.GetSingleEvent<StaticTextUpdated>().Text.ShouldEqual(text);

        It should_raise_StaticTextUpdated_event_with_enablement_condition_specified = () =>
            eventContext.GetSingleEvent<StaticTextUpdated>().EnablementCondition.ShouldEqual(enablementCondition);

        private It should_raise_StaticTextUpdated_event_with_hide_if_disabled_specified = () =>
            eventContext.GetSingleEvent<StaticTextUpdated>().HideIfDisabled.ShouldBeTrue();

        private It should_raise_StaticTextUpdated_event_with_validation_condition_count_equals_1 = () =>
            eventContext.GetSingleEvent<StaticTextUpdated>().ValidationConditions.Count.ShouldEqual(1);

        private It should_raise_StaticTextUpdated_event_with_validation_condition_expression_specified = () =>
            eventContext.GetSingleEvent<StaticTextUpdated>().ValidationConditions.Single().Expression.ShouldEqual(validationCondition);

        private It should_raise_StaticTextUpdated_event_with_validation_condition_message_specified = () =>
            eventContext.GetSingleEvent<StaticTextUpdated>().ValidationConditions.Single().Message.ShouldEqual(validationMessage);

        private static UpdateStaticText command;
        private static EventContext eventContext;
        private static Questionnaire questionnaire;
        private static Guid entityId = Guid.Parse("11111111111111111111111111111111");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static string text = "some text";
        private static string enablementCondition = "condition";


        private static string validationCondition = "some condition";

        private static string validationMessage = "some message";
    }
}