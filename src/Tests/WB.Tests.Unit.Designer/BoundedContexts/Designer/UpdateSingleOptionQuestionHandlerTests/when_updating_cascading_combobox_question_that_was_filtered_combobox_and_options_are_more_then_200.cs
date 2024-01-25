using System;
using System.Linq;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;
using WB.Tests.Unit.Designer.QuestionnaireVerificationTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.UpdateSingleOptionQuestionHandlerTests
{
    internal class when_updating_cascading_combobox_question_that_was_filtered_combobox_and_options_are_more_then_200 : QuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            int incrementer = 0;
            oldOptions = new Option[210].Select(
                answer =>
                    new Option(
                        value : incrementer.ToString(),
                        title : (incrementer++).ToString()
                    )).ToArray();

            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
            questionnaire.AddSingleOptionQuestion(
                parentQuestionId,
                chapterId,
                options : new Option[] { new Option (title : "option1", value : "1" ), new Option (title : "option2", value : "2" ) },
                title : "Parent question",
                variableName : "cascade_parent",
                responsibleId : responsibleId,
                linkedToQuestionId : null,
                isFilteredCombobox : false,
                cascadeFromQuestionId : null
            );

            questionnaire.AddSingleOptionQuestion
            (
                filteredQuestionId,
                chapterId,
                options : oldOptions,
                title : "Filtered combobox question",
                variableName: "filtered",
                responsibleId : responsibleId,
                linkedToQuestionId : null,
                isFilteredCombobox : true,
                cascadeFromQuestionId : null
            );

            questionnaire.UpdateFilteredComboboxOptions(filteredQuestionId, responsibleId, 
                oldOptions.Select(x=> Create.QuestionnaireCategoricalOption((int)x.Value, x.Title)).ToArray());
            BecauseOf();
        }

        private void BecauseOf() =>
            questionnaire.UpdateSingleOptionQuestion(
                new UpdateSingleOptionQuestion(
                questionnaireId: questionnaire.Id,
                questionId: filteredQuestionId,
                commonQuestionParameters:new CommonQuestionParameters()
                {
                    Title = "title",
                    VariableName = "qr_barcode_question",
                    VariableLabel = null,
                    EnablementCondition = null,
                    Instructions = "instructions",
                },
                
                isPreFilled: false,
                scope: QuestionScope.Interviewer,
                responsibleId: responsibleId,
                options: null,
                linkedToEntityId: (Guid?)null,
                isFilteredCombobox: false,
                cascadeFromQuestionId: parentQuestionId, 
                validationConditions: new System.Collections.Generic.List<WB.Core.SharedKernels.QuestionnaireEntities.ValidationCondition>(),
                linkedFilterExpression: null,
                validationExpression: null,
                validationMessage: null,
                showAsList:false,
                showAsListThreshold: null,
                categoriesId: null));

        [NUnit.Framework.Test] public void should_raise_QuestionChanged_event_with_answer_option_that_was_presiously_saved () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(filteredQuestionId)
                .Answers.Count().Should().Be(oldOptions.Count());

        private static Questionnaire questionnaire;
        private static Guid filteredQuestionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid parentQuestionId = Guid.Parse("22222222222222222222222222222222");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Option[] oldOptions;
    }
}
