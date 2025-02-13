using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.StaticText;

using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.UpdateStaticTextHandlerTests
{
    internal class when_updating_static_text_and_validation_message_has_tags : QuestionnaireTestsContext
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
                validationConditions: new List<ValidationCondition>() { new ValidationCondition(validationCondition, validationMessageWithValidTags), 
                    new ValidationCondition(validationCondition, validationMessageWithInvalidTags) }
                );
            BecauseOf();
        }

        private void BecauseOf() =>            
                questionnaire.UpdateStaticText(command);
        [NUnit.Framework.Test] public void should_contains_statictext () =>
            questionnaire.QuestionnaireDocument.Find<IStaticText>(entityId).Should().NotBeNull();

        [NUnit.Framework.Test] public void should_contains_statictext_with_validation_condition_count_equals_2 () =>
            questionnaire.QuestionnaireDocument.Find<IStaticText>(entityId).ValidationConditions.Count.Should().Be(2);

        [NUnit.Framework.Test] public void should_contains_statictext_with_validation_condition_message1_specified () =>
            questionnaire.QuestionnaireDocument.Find<IStaticText>(entityId).ValidationConditions.First().Message.Should().Be(validationMessageWithValidTags);

        [NUnit.Framework.Test] public void should_contains_statictext_with_validation_condition_message2_specified () =>
            questionnaire.QuestionnaireDocument.Find<IStaticText>(entityId).ValidationConditions.Last().Message.Should().Be(validationMessageSanitized);
        
        private static UpdateStaticText command;
        private static Questionnaire questionnaire;
        private static Guid entityId = Guid.Parse("11111111111111111111111111111112");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static string text = "some text";
        private static string enablementCondition = "condition";


        private static string validationCondition = "some condition";

        private static string validationMessageWithValidTags = "some message <b>test</b>";
        
        private static string validationMessageWithInvalidTags = "some message <a>test > 1</a>";
        private static string validationMessageSanitized = "some message test > 1";
    }
}
