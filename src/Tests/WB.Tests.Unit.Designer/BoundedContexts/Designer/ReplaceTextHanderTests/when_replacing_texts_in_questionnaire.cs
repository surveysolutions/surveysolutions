using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.ReplaceTextHanderTests
{
    internal class when_replacing_texts_in_questionnaire : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            var responsibleId = Guid.Parse("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb");
            questionnaire = CreateQuestionnaireWithOneGroup(responsibleId: responsibleId,
                groupId: chapterId);

            questionnaire.AddStaticText(Create.Event.StaticTextAdded(entityId: staticTextId,
                text: $"static text title with {searchFor}",
                parentId: chapterId,
                enablementCondition: $"static text enablement {searchFor}",
                validationConditions: new List<ValidationCondition>
                {
                    Create.ValidationCondition($"st validation exp {searchFor}", message: $"st validation msg {searchFor}")
                }));

            questionnaire.AddQuestion(Create.Event.NewQuestionAdded(questionId, 
                questionText: $"question title with {searchFor}",
                groupPublicKey: chapterId,
                conditionExpression: $"question enablement {searchFor}",
                validationConditions: new List<ValidationCondition>
                {
                    Create.ValidationCondition($"q validation exp {searchFor}", message: $"q validation msg {searchFor}")
                }));

            questionnaire.AddGroup(Create.Event.NewGroupAddedEvent(groupId.FormatGuid(), 
                parentGroupId: chapterId.FormatGuid(), 
                groupTitle: $"group title with {searchFor}",
                enablementCondition: $"group enablement {searchFor}"
                ));

            questionnaire.AddMacro(Create.Event.MacroAdded(questionnaire.Id, macroId, responsibleId));
            questionnaire.UpdateMacro(Create.Event.MacroUpdated(questionId, macroId, "macro_name", 
                $"macro content {searchFor}", "desc", responsibleId));

            command = Create.Command.ReplaceTextsCommand(searchFor, replaceWith, matchCase: true);
        };

        Because of = () => questionnaire.ReplaceTexts(command);

        It should_replace_title_of_static_text = () => 
            questionnaire.QuestionnaireDocument.Find<StaticText>(staticTextId).GetTitle().ShouldEqual($"static text title with {replaceWith}");

        It should_replace_enablement_condition_of_static_text = () => 
            questionnaire.QuestionnaireDocument.Find<StaticText>(staticTextId).ConditionExpression.ShouldEqual($"static text enablement {replaceWith}");

        It should_replace_validation_condition_of_static_text = () =>
            questionnaire.QuestionnaireDocument.Find<StaticText>(staticTextId).ValidationConditions.First().Expression.ShouldEqual($"st validation exp {replaceWith}");

        It should_replace_validation_message_of_static_text = () =>
            questionnaire.QuestionnaireDocument.Find<StaticText>(staticTextId).ValidationConditions.First().Message.ShouldEqual($"st validation msg {replaceWith}");

        It should_replace_title_of_question = () => 
            questionnaire.QuestionnaireDocument.Find<IComposite>(questionId).GetTitle().ShouldEqual($"question title with {replaceWith}");

        It should_replace_enablement_condition_of_question = () => 
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId).ConditionExpression.ShouldEqual($"question enablement {replaceWith}");

        It should_replace_validation_condition_of_question = () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId).ValidationConditions.First().Expression.ShouldEqual($"q validation exp {replaceWith}");

        It should_replace_validation_message_of_question = () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId).ValidationConditions.First().Message.ShouldEqual($"q validation msg {replaceWith}");

        It should_replace_title_of_group = () => 
            questionnaire.QuestionnaireDocument.Find<IComposite>(groupId).GetTitle().ShouldEqual($"group title with {replaceWith}");

        It should_replace_enablement_condition_of_group = () => 
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId).ConditionExpression.ShouldEqual($"group enablement {replaceWith}");

        It should_replace_content_of_macro = () =>
            questionnaire.QuestionnaireDocument.Macros[macroId].Content.ShouldEqual($"macro content {replaceWith}");

        static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        static Questionnaire questionnaire;

        static readonly Guid staticTextId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
        static readonly Guid questionId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        static readonly Guid groupId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        static readonly Guid macroId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
        private static ReplaceTextsCommand command;
        const string replaceWith = "replaced";
        const string searchFor = "%to replace%";
    }
}