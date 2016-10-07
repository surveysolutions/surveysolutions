using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.ReplaceTextHanderTests
{
    internal class when_searching_all_texts_in_questionnaire : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            var responsibleId = Guid.Parse("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb");
            questionnaire = CreateQuestionnaireWithOneGroup(responsibleId: responsibleId,
                groupId: chapterId);

            questionnaire.AddStaticTextAndMoveIfNeeded(Create.Command.AddStaticText(questionnaire.Id, staticTextId, $"static text title with {searchFor}", responsibleId, chapterId));
            
            questionnaire.AddTextQuestion(questionId,
                chapterId,
                responsibleId,
                title: $"question title with {searchFor}",
                enablementCondition: $"question enablement {searchFor}",
                validationConditions: new List<ValidationCondition>
                {
                    Create.ValidationCondition($"q validation exp {searchFor}", message: $"q validation msg {searchFor}")
                });

            questionnaire.AddQuestion(Create.Event.NewQuestionAdded(questionId1,
                stataExportCaption: $"variable_{searchFor}",
                groupPublicKey: chapterId));
             

            questionnaire.AddQuestion(Create.Event.NewQuestionAdded(questionId2,
                questionType: QuestionType.MultyOption,
                answers: new Answer[]
                {
                    Create.Answer($"answer with {searchFor}")
                }));

            questionnaire.AddVariable(Create.Event.VariableAdded(variableId,
                variableExpression: $"expression {searchFor}",
                parentId: chapterId));

            questionnaire.AddGroup(Create.Event.NewGroupAddedEvent(groupId.FormatGuid(),
                parentGroupId: chapterId.FormatGuid(),
                groupTitle: $"group title with {searchFor}",
                enablementCondition: $"group enablement {searchFor}"
                ));

            questionnaire.AddMacro(Create.Command.AddMacro(questionnaire.Id, macroId, responsibleId));


            questionnaire.UpdateMacro(Create.Command.UpdateMacro(questionId, macroId, "macro_name",
                $"macro content {searchFor}", "desc", responsibleId));
        };

        Because of = () => foundReferences = questionnaire.FindAllTexts(searchFor, true, false, false);

        It should_find_text_in_static_text = () =>
            foundReferences.ShouldContain(x => x.Id == staticTextId && x.Type == QuestionnaireVerificationReferenceType.StaticText);

        It should_find_text_in_question = () =>
            foundReferences.ShouldContain(x => x.Id == questionId && x.Type == QuestionnaireVerificationReferenceType.Question);

        It should_find_text_in_group = () =>
            foundReferences.ShouldContain(x => x.Id == groupId && x.Type == QuestionnaireVerificationReferenceType.Group);

        It should_not_duplicate_found_entities = () =>
            foundReferences.Count(x => x.Id == staticTextId).ShouldEqual(1);

        It should_find_text_in_variable_name = () =>
            foundReferences.ShouldContain(x => x.Id == questionId1);

        It should_find_variables_by_content = () => 
            foundReferences.ShouldContain(x => x.Id == variableId && x.Type == QuestionnaireVerificationReferenceType.Variable);

        It should_find_question_by_option_text = () => 
            foundReferences.ShouldContain(x => x.Id == questionId2);

        static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        static Questionnaire questionnaire;

        static readonly Guid staticTextId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
        static readonly Guid questionId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        static readonly Guid questionId1 = Guid.Parse("11111111111111111111111111111111");
        static readonly Guid questionId2 = Guid.Parse("33333333333333333333333333333333");
        static readonly Guid variableId = Guid.Parse("22222222222222222222222222222222");
        static readonly Guid groupId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        static readonly Guid macroId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
        const string searchFor = "%to replace%";

        private static IEnumerable<QuestionnaireNodeReference> foundReferences;
    }
}