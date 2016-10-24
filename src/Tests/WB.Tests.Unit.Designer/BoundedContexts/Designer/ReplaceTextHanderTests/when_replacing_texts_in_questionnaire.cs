using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.StaticText;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.ReplaceTextHanderTests
{
    internal class when_replacing_texts_in_questionnaire : QuestionnaireTestsContext
    {
        private Establish context = () =>
        {
            responsibleId = Guid.Parse("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb");
            questionnaire = CreateQuestionnaireWithOneGroup(responsibleId: responsibleId,
                groupId: chapterId);

            questionnaire.AddStaticTextAndMoveIfNeeded(new AddStaticText(questionnaire.Id, staticTextId, "static text", responsibleId, chapterId));
            questionnaire.UpdateStaticText(new UpdateStaticText(questionnaire.Id, staticTextId, $"static text title with {searchFor}", null, 
                responsibleId, $"static text enablement {searchFor}", false, new List<ValidationCondition>
                {
                    Create.ValidationCondition($"st validation exp {searchFor}", message: $"st validation msg {searchFor}")
                }));

            questionnaire.AddTextQuestion(questionId, 
                chapterId,
                responsibleId,
                title: $"question title with {searchFor}",
                enablementCondition: $"question enablement {searchFor}",
                validationConditions: new List<ValidationCondition>
                {
                    Create.ValidationCondition($"q validation exp {searchFor}", message: $"q validation msg {searchFor}")
                });

            questionnaire.AddMultiOptionQuestion(questionId1,
                variableName: $"var_{searchFor}",
                responsibleId:responsibleId,
                parentGroupId: chapterId,
                options: new []
                {
                    new Option(Guid.NewGuid() ,"1",$"answer with {searchFor}"),
                    new Option(Guid.NewGuid() ,"2",$"2")
                });

            questionnaire.AddVariable(
                variableId,
                responsibleId:responsibleId,
                variableExpression: $"var expression {searchFor}",
                parentId: chapterId);

            questionnaire.AddGroup(groupId, chapterId, 
                title: $"group title with {searchFor}",
                enablingCondition: $"group enablement {searchFor}", responsibleId: responsibleId);

            questionnaire.AddMacro(Create.Command.AddMacro(questionnaire.Id, macroId, responsibleId));

            questionnaire.UpdateMacro(Create.Command.UpdateMacro(questionId, macroId, "macro_name", 
                $"macro content {searchFor}", "desc", responsibleId));

            command = Create.Command.ReplaceTextsCommand(searchFor, replaceWith, matchCase: true, userId: responsibleId);
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

        It should_repalce_variable_expressions = () => 
            questionnaire.QuestionnaireDocument.Find<IVariable>(variableId).Expression.ShouldEqual($"var expression {replaceWith}");

        It should_replace_variable_names = () => 
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId1).StataExportCaption.ShouldEqual($"var_{replaceWith}");

        It should_replace_option_text = () => 
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId1).Answers.First().AnswerText.ShouldEqual($"answer with {replaceWith}");

        It should_record_number_of_affected_entities = () => 
            questionnaire.GetLastReplacedEntriesCount().ShouldEqual(6);

        static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        static Questionnaire questionnaire;

        static readonly Guid staticTextId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
        static readonly Guid questionId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        static readonly Guid questionId1 = Guid.Parse("11111111111111111111111111111111");
        static readonly Guid variableId = Guid.Parse("22222222222222222222222222222222");
        static readonly Guid groupId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        static readonly Guid macroId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
        private static ReplaceTextsCommand command;
        private static Guid responsibleId;
        const string replaceWith = "replaced";
        const string searchFor = "to_replace";
    }
}