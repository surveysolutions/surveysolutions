using System;
using System.Linq;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.UpdateSingleOptionQuestionHandlerTests
{
    internal class  UpdateOptionsTests: QuestionnaireTestsContext
    {
        [Test]
        public void when_updating_filtered_combobox_question_that_was_cascading_combobox_and_options_are_more_then_200()
        {
            Guid cascadeQuestionId = Guid.Parse("11111111111111111111111111111111");
            Guid parentQuestionId = Guid.Parse("22222222222222222222222222222222");
            Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");

            int incrementer = 0;
            var oldOptions = new Option[210].Select(
                answer =>
                    new Option
                    (
                        value : incrementer.ToString(),
                        title : (incrementer++).ToString(),
                        parentValue : "1"
                    )).ToArray();

            var questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId: responsibleId);
            questionnaire.AddSingleOptionQuestion
            (
                parentQuestionId,
                chapterId,
                options: new Option[] { new Option (title : "option1", value : "1"),
                    new Option(title: "option2", value : "2")},
                title: "Parent question",
                variableName: "cascade_parent",
                isPreFilled: false,
                responsibleId: responsibleId,
                linkedToQuestionId: null,
                isFilteredCombobox: false,
                cascadeFromQuestionId: null
            );

            questionnaire.AddSingleOptionQuestion
            (
                cascadeQuestionId,
                chapterId,
                options: oldOptions,
                title: "Cascade question",
                variableName: "cascade",
                isPreFilled: false,
                responsibleId: responsibleId,
                linkedToQuestionId: null,
                isFilteredCombobox: false,
                cascadeFromQuestionId: parentQuestionId
            );

            questionnaire.UpdateCascadingComboboxOptions(cascadeQuestionId, responsibleId, 
                oldOptions.Select(x => Create.QuestionnaireCategoricalOption(int.Parse(x.Value), x.Title)).ToArray());

            //act
            questionnaire.UpdateSingleOptionQuestion(
                new UpdateSingleOptionQuestion(
                    questionnaireId: questionnaire.Id,
                    questionId: cascadeQuestionId,
                    commonQuestionParameters: new CommonQuestionParameters()
                    {
                        Title = "title",
                        VariableName = "qr_barcode_question",
                        VariableLabel = null,
                        EnablementCondition = "some condition",
                        Instructions = "instructions",
                        HideIfDisabled = false
                    },

                    isPreFilled: false,
                    scope: QuestionScope.Interviewer,
                    responsibleId: responsibleId,
                    options: null,
                    linkedToEntityId: (Guid?)null,
                    isFilteredCombobox: true,
                    cascadeFromQuestionId: null,
                    validationConditions: new System.Collections.Generic.List<WB.Core.SharedKernels.QuestionnaireEntities.ValidationCondition>(),
                    linkedFilterExpression: null,
                    validationExpression: null,
                    validationMessage: null,
                    showAsList: false,
                    showAsListThreshold: null,
                    categoriesId: null));

            //assert

            Assert.That(questionnaire.QuestionnaireDocument.Find<IQuestion>(cascadeQuestionId), Is.Not.Null);
            Assert.That(questionnaire.QuestionnaireDocument.Find<IQuestion>(cascadeQuestionId)
                .Answers.Count, Is.EqualTo(oldOptions.Count()));
        }

        [Test]
        public void when_updating_filtered_combobox_question_show_as_list_should_be_preserved()
        {
            Guid cascadeQuestionId = Guid.Parse("11111111111111111111111111111111");
            Guid parentQuestionId = Guid.Parse("22222222222222222222222222222222");
            Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");

            int incrementer = 0;
            var oldOptions = new Option[10].Select(
                answer =>
                    new Option
                    (
                        value : incrementer.ToString(),
                        title : (incrementer++).ToString(),
                        parentValue : "1"
                    )).ToArray();

            var questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId: responsibleId);
            questionnaire.AddSingleOptionQuestion
            (
                parentQuestionId,
                chapterId,
                options: new Option[] { new Option (title : "option1", value : "1"),
                    new Option(title: "option2", value : "2")},
                title: "Parent question",
                variableName: "cascade_parent",
                isPreFilled: false,
                responsibleId: responsibleId,
                linkedToQuestionId: null,
                isFilteredCombobox: false,
                cascadeFromQuestionId: null
                
            );

            questionnaire.AddSingleOptionQuestion
            (
                cascadeQuestionId,
                chapterId,
                options: oldOptions,
                title: "Cascade question",
                variableName: "cascade",
                isPreFilled: false,
                responsibleId: responsibleId,
                linkedToQuestionId: null,
                isFilteredCombobox: false,
                cascadeFromQuestionId: parentQuestionId,
                showAsList: true,
                showAsListThreshold:5
            );

            //act
            questionnaire.UpdateCascadingComboboxOptions(cascadeQuestionId, responsibleId,
                oldOptions.Select(x => Create.QuestionnaireCategoricalOption(int.Parse(x.Value), x.Title)).ToArray());

            

            //assert

            Assert.That(questionnaire.QuestionnaireDocument.Find<SingleQuestion>(cascadeQuestionId).ShowAsList, Is.True);
            Assert.That(questionnaire.QuestionnaireDocument.Find<SingleQuestion>(cascadeQuestionId).ShowAsListThreshold, Is.EqualTo(5));
        }
    }
}
