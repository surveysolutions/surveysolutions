using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Abc;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.QuestionnaireTests.CascadingOptions
{
    internal class CascadingOptionTests : QuestionnaireTestsContext
    {
        [Test]
        public void when_updating_question_with_show_as_list_limit()
        {
            var rootGroupId = Guid.Parse("00000000000000000000000000000000");
            var actorId = Guid.Parse("11111111111111111111111111111111");
            var questionnaire = CreateQuestionnaireWithOneGroup(actorId, groupId: rootGroupId);

            var parentQuestionId = Guid.Parse("22222222222222222222222222222222");
            var updatedQuestionId = Guid.Parse("33333333333333333333333333333333");

            questionnaire.AddSingleOptionQuestion(parentQuestionId, rootGroupId, actorId,
                options: new [] {
                    new Option(title : "one", value : "1"),
                    new Option(title : "two", value : "2")
                });

            questionnaire.AddSingleOptionQuestion(updatedQuestionId, rootGroupId, actorId,
                options: new [] {
                    new Option(title : "one",value : "1"),
                    new Option(title : "two",value : "2")
                });

            //Act
            questionnaire.UpdateSingleOptionQuestion(
                new UpdateSingleOptionQuestion(questionnaireId: questionnaire.Id,
                    questionId: updatedQuestionId,
                    commonQuestionParameters: new CommonQuestionParameters()
                    {
                        Title = "title",
                        VariableName = "var",
                        VariableLabel = null,
                        HideIfDisabled = false
                    },
                    isPreFilled: false,
                    scope: QuestionScope.Interviewer,
                    responsibleId: actorId,
                    options: new[]
                    {
                        new Option(title : "one",value : "1"),
                        new Option(title : "two",value : "2")
                    },
                    linkedToEntityId: null,
                    isFilteredCombobox: false,
                    cascadeFromQuestionId: parentQuestionId,
                    validationConditions: new List<ValidationCondition>(),
                    linkedFilterExpression: null,
                    validationExpression: null,
                    validationMessage: null,
                    showAsList: true,
                    showAsListThreshold: 20,
                    categoriesId: null));

            //AAA
            var categoricalQuestion = (questionnaire.QuestionnaireDocument.Find<IQuestion>(updatedQuestionId) as SingleQuestion);

            Assert.That(categoricalQuestion.ShowAsList, Is.True);
            Assert.That(categoricalQuestion.ShowAsListThreshold, Is.EqualTo(20));
        }

        [Test]
        public void when_updating_filtered_combobox_options_with_negative_values()
        {
            var questionId = Id.g1;
            var questionnaireDocument =
                Create.QuestionnaireDocumentWithOneChapter(
                    Create.SingleOptionQuestion(
                        questionId: questionId,
                        isComboBox: true
                    ));

            var questionnaire = Create.Questionnaire(Id.gA, questionnaireDocument);

            // Act
            questionnaire.UpdateFilteredComboboxOptions(questionId, Id.gA, new[]
            {
                Create.QuestionnaireCategoricalOption(-1, "m 1")
            });

            var categoricalQuestion = questionnaireDocument.Find<ICategoricalQuestion>(questionId);
            Assert.That(categoricalQuestion.Answers, Has.Count.EqualTo(1));
            Assert.That(categoricalQuestion.Answers[0], Has.Property(nameof(Answer.AnswerCode)).EqualTo(-1));
        }

        [Test]
        public void when_updating_question_with_empty_cascading_options()
        {
            var rootGroupId = Guid.Parse("00000000000000000000000000000000");
            var actorId = Guid.Parse("11111111111111111111111111111111");
            var questionnaire = CreateQuestionnaireWithOneGroup(actorId, groupId: rootGroupId);

            var parentQuestionId = Guid.Parse("22222222222222222222222222222222");
            var updatedQuestionId = Guid.Parse("33333333333333333333333333333333");

            questionnaire.AddSingleOptionQuestion(parentQuestionId, rootGroupId, actorId,
                options: new [] 
                {   new Option(title : "one", value : "1"),
                    new Option(title : "two", value : "2")
                });

            questionnaire.AddSingleOptionQuestion(updatedQuestionId, rootGroupId, actorId,
                options: new [] 
                {   new Option(title : "one", value : "1"),
                    new Option(title : "two", value : "2")});

            //Act
            questionnaire.UpdateSingleOptionQuestion(
            new UpdateSingleOptionQuestion(questionnaireId: questionnaire.Id,
                questionId: updatedQuestionId,
                commonQuestionParameters: new CommonQuestionParameters()
                {
                    Title = "title",
                    VariableName = "var",
                    VariableLabel = null,
                    HideIfDisabled = false
                },

                isPreFilled: false,
                scope: QuestionScope.Interviewer,
                responsibleId: actorId,
                options: new[]
                {
                    new Option(null, String.Empty, (decimal?)null),
                    new Option(null, String.Empty, (decimal?)null),
                    new Option(null, String.Empty, (decimal?)null)
                },
                linkedToEntityId: null,
                isFilteredCombobox: false,
                cascadeFromQuestionId: parentQuestionId,
                validationConditions: new System.Collections.Generic.List<WB.Core.SharedKernels.QuestionnaireEntities.ValidationCondition>(),
                linkedFilterExpression: null,
                validationExpression: null,
                validationMessage: null,
                showAsList: false,
                showAsListThreshold: null,
                categoriesId: null));
            
            Assert.That(questionnaire.QuestionnaireDocument.Find<IQuestion>(updatedQuestionId).Answers.Count, Is.EqualTo(2));
        }
    }
}

