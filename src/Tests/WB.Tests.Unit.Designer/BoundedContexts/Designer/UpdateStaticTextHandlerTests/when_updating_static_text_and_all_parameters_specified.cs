using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.StaticText;

using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.UpdateStaticTextHandlerTests
{
    internal class when_updating_static_text_and_all_parameters_specified : QuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
            questionnaire.AddStaticTextAndMoveIfNeeded(new AddStaticText(questionnaire.Id, entityId, "static text", responsibleId, chapterId));

            command = Create.Command.UpdateStaticText(
                questionnaire.Id,
                entityId: entityId,
                text: text,
                attachmentName: "",
                enablementCondition : enablementCondition,
                responsibleId: responsibleId,
                hideIfDisabled: true,
                validationConditions: new List<ValidationCondition>() { new ValidationCondition(validationCondition, validationMessage) }
                );
        }

        private void BecauseOf() =>            
                questionnaire.UpdateStaticText(command);



        [NUnit.Framework.Test] public void should_contains_statictext () =>
            questionnaire.QuestionnaireDocument.Find<IStaticText>(entityId).ShouldNotBeNull();

        [NUnit.Framework.Test] public void should_contains_statictext_with_EntityId_specified () =>
            questionnaire.QuestionnaireDocument.Find<IStaticText>(entityId).PublicKey.ShouldEqual(entityId);

        [NUnit.Framework.Test] public void should_contains_statictext_with_Text_specified () =>
            questionnaire.QuestionnaireDocument.Find<IStaticText>(entityId).Text.ShouldEqual(text);

        [NUnit.Framework.Test] public void should_contains_statictext_with_enablement_condition_specified () =>
            questionnaire.QuestionnaireDocument.Find<IStaticText>(entityId).ConditionExpression.ShouldEqual(enablementCondition);

        private [NUnit.Framework.Test] public void should_contains_statictext_with_hide_if_disabled_specified () =>
            questionnaire.QuestionnaireDocument.Find<IStaticText>(entityId).HideIfDisabled.ShouldBeTrue();

        private [NUnit.Framework.Test] public void should_contains_statictext_with_validation_condition_count_equals_1 () =>
            questionnaire.QuestionnaireDocument.Find<IStaticText>(entityId).ValidationConditions.Count.ShouldEqual(1);

        private [NUnit.Framework.Test] public void should_contains_statictext_with_validation_condition_expression_specified () =>
            questionnaire.QuestionnaireDocument.Find<IStaticText>(entityId).ValidationConditions.Single().Expression.ShouldEqual(validationCondition);

        private [NUnit.Framework.Test] public void should_contains_statictext_with_validation_condition_message_specified () =>
            questionnaire.QuestionnaireDocument.Find<IStaticText>(entityId).ValidationConditions.Single().Message.ShouldEqual(validationMessage);

        private static UpdateStaticText command;
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