using System;
using System.Collections.Generic;
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

            questionnaire.AddTextQuestion(questionId1,
                chapterId,
                responsibleId,
                variableName: $"var_{searchFor}");
             
            questionnaire.AddMultiOptionQuestion(questionId2,
                chapterId,
                responsibleId,
                options: new []
                {
                    new Option(Guid.NewGuid(),"1", $"1"),
                    new Option(Guid.NewGuid(),"2", $"answer with {searchFor}")
                });

            questionnaire.AddSingleOptionQuestion(filteredQuestionId,
                chapterId,
                responsibleId,
                isFilteredCombobox: true,
                options: new[]
                {
                                new Option(Guid.NewGuid(),"1", $"1"),
                                new Option(Guid.NewGuid(),"2", $"answer with {searchFor}")
                });
            
            questionnaire.AddSingleOptionQuestion(cascadingQuestionId,
             chapterId,
             responsibleId,
             cascadeFromQuestionId: filteredQuestionId,
             options: new[]
             {
                                new Option(Guid.NewGuid(),"1", $"1"),
                                new Option(Guid.NewGuid(),"2", $"answer with {searchFor}")
             });

            questionnaire.AddVariable(
                variableId,
                responsibleId:responsibleId,
                variableExpression: $"expression {searchFor}",
                parentId: chapterId);

            questionnaire.AddGroup(groupId,
                chapterId,
                title: $"group title with {searchFor}",
                enablingCondition: $"group enablement {searchFor}", responsibleId: responsibleId);

            questionnaire.AddMacro(Create.Command.AddMacro(questionnaire.Id, macroId, responsibleId));


            questionnaire.UpdateMacro(Create.Command.UpdateMacro(questionId, macroId, "macro_name",
                $"macro content {searchFor}", "desc", responsibleId));
        };

        Because of = () => foundReferences = questionnaire.FindAllTexts(searchFor, true, false, false);

        It should_find_text_in_static_text = () =>
            foundReferences.ShouldContain(x => x.Id == staticTextId && 
                                               x.Type == QuestionnaireVerificationReferenceType.StaticText &&
                                               x.Property == QuestionnaireVerificationReferenceProperty.Title);

        It should_find_text_in_question_title = () =>
            foundReferences.ShouldContain(x => x.Id == questionId && 
                                               x.Type == QuestionnaireVerificationReferenceType.Question &&
                                               x.Property == QuestionnaireVerificationReferenceProperty.Title);

        It should_find_text_in_question_validation_expression = () =>
          foundReferences.ShouldContain(x => x.Id == questionId &&
                                             x.Type == QuestionnaireVerificationReferenceType.Question &&
                                             x.Property == QuestionnaireVerificationReferenceProperty.ValidationExpression);

        It should_find_text_in_question_validation_message = () =>
         foundReferences.ShouldContain(x => x.Id == questionId &&
                                            x.Type == QuestionnaireVerificationReferenceType.Question &&
                                            x.Property == QuestionnaireVerificationReferenceProperty.ValidationMessage);

        It should_find_text_in_question_enablement_condition = () =>
        foundReferences.ShouldContain(x => x.Id == questionId &&
                                           x.Type == QuestionnaireVerificationReferenceType.Question &&
                                           x.Property == QuestionnaireVerificationReferenceProperty.EnablingCondition);

        It should_find_text_in_group = () =>
            foundReferences.ShouldContain(x => x.Id == groupId && x.Type == QuestionnaireVerificationReferenceType.Group && 
                                               x.Property == QuestionnaireVerificationReferenceProperty.Title);

        It should_find_text_in_variable_name = () =>
            foundReferences.ShouldContain(x => x.Id == questionId1 && 
                                               x.Property == QuestionnaireVerificationReferenceProperty.VariableName);

        It should_find_variables_by_content = () => 
            foundReferences.ShouldContain(x => x.Id == variableId && 
                                               x.Type == QuestionnaireVerificationReferenceType.Variable && 
                                               x.Property == QuestionnaireVerificationReferenceProperty.VariableContent);

        It should_find_question_by_option_text = () => 
            foundReferences.ShouldContain(x => x.Id == questionId2 && 
                                               x.Property == QuestionnaireVerificationReferenceProperty.Option &&
                                               x.IndexOfEntityInProperty == 1);

        It should_not_include_references_to_filtered_combobox_options = () => 
            foundReferences.ShouldNotContain(x => x.Id == filteredQuestionId);

        It should_not_include_references_to_cascading_combobox_options = () =>
            foundReferences.ShouldNotContain(x => x.Id == cascadingQuestionId);

        static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        static Questionnaire questionnaire;

        static readonly Guid staticTextId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
        static readonly Guid questionId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        static readonly Guid questionId1 = Guid.Parse("11111111111111111111111111111111");
        static readonly Guid questionId2 = Guid.Parse("33333333333333333333333333333333");
        static readonly Guid filteredQuestionId = Guid.Parse("44444444444444444444444444444444");
        static readonly Guid cascadingQuestionId = Guid.Parse("55555555555555555555555555555555");
        static readonly Guid variableId = Guid.Parse("22222222222222222222222222222222");
        static readonly Guid groupId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        static readonly Guid macroId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
        const string searchFor = "to_replace";

        private static IEnumerable<QuestionnaireNodeReference> foundReferences;
    }
}