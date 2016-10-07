using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.UpdateSingleOptionQuestionHandlerTests
{
    internal class when_updating_cascading_combobox_question_that_was_filtered_combobox_and_options_are_more_then_200 : QuestionnaireTestsContext
    {
        private Establish context = () =>
        {
            int incrementer = 0;
            oldOptions = new Option[210].Select(
                answer =>
                    new Option()
                    {
                        Value = incrementer.ToString(),
                        Title = (incrementer++).ToString()
                    }).ToArray();

            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.AddSingleOptionQuestion(
                parentQuestionId,
                chapterId,
                options : new Option[] { new Option { Title = "option1", Value = "1" }, new Option { Title = "option2", Value = "2" } },
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

            questionnaire.UpdateFilteredComboboxOptions(filteredQuestionId, responsibleId, oldOptions);
        };

        Because of = () =>
            questionnaire.UpdateSingleOptionQuestion(
                questionId: filteredQuestionId,
                title: "title",
                variableName: "qr_barcode_question",
                variableLabel: null,
                isPreFilled: false,
                scope: QuestionScope.Interviewer,
                enablementCondition: null,
                hideIfDisabled: false,
                instructions: "intructions",
                responsibleId: responsibleId,
                options: null,
                linkedToEntityId: (Guid?)null,
                isFilteredCombobox: false,
                cascadeFromQuestionId: parentQuestionId, validationConditions: new System.Collections.Generic.List<WB.Core.SharedKernels.QuestionnaireEntities.ValidationCondition>(),
                linkedFilterExpression: null, properties: Create.QuestionProperties());


   
        It should_raise_QuestionChanged_event_with_answer_option_that_was_presiously_saved = () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(filteredQuestionId)
                .Answers.Count().ShouldEqual(oldOptions.Count());

        private static Questionnaire questionnaire;
        private static Guid filteredQuestionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid parentQuestionId = Guid.Parse("22222222222222222222222222222222");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Option[] oldOptions;
    }
}