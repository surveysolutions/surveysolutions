using System;
using System.Linq;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests.CascadigOptions
{
    internal class when_updating_question_with_empty_cascading_options : QuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            rootGroupId = Guid.Parse("00000000000000000000000000000000");
            actorId = Guid.Parse("11111111111111111111111111111111");
            questionnaire = CreateQuestionnaireWithOneGroup(actorId, groupId: rootGroupId);

            parentQuestionId = Guid.Parse("22222222222222222222222222222222");
            updatedQuestionId = Guid.Parse("33333333333333333333333333333333");

            questionnaire.AddSingleOptionQuestion(
                parentQuestionId,
                rootGroupId,
                actorId,
                options : new Option[] {
                    new Option()
                    {
                        Title = "one",
                        Value = "1"
                    },
                    new Option()
                    {
                        Title = "two",
                        Value = "2"
                    }
                }
            );

            questionnaire.AddSingleOptionQuestion(updatedQuestionId,
                rootGroupId,
                actorId,
                options: new Option[] {
                    new Option{Title = "one",Value = "1"},
                    new Option{Title = "two",Value = "2"}
                });
            BecauseOf();
        }

        private void BecauseOf() => questionnaire.UpdateSingleOptionQuestion(


            new UpdateSingleOptionQuestion(
                questionnaireId: questionnaire.Id,
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
                    new Option(String.Empty, String.Empty, (decimal?)null),
                    new Option(String.Empty, String.Empty, (decimal?)null),
                    new Option(String.Empty, String.Empty, (decimal?)null)
                },
                linkedToEntityId: null,
                isFilteredCombobox: false,
                cascadeFromQuestionId: parentQuestionId,
                validationConditions: new System.Collections.Generic.List<WB.Core.SharedKernels.QuestionnaireEntities.ValidationCondition>(),
                linkedFilterExpression: null,
                validationExpression: null,
                validationMessage: null,
                showAsList: false,
                showAsListLimit: null));


        [NUnit.Framework.Test] public void should_contains_question_with_empty_answers () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(updatedQuestionId).Answers.Count().Should().Be(2);


        private static Questionnaire questionnaire;
        private static Guid parentQuestionId;
        private static Guid rootGroupId;
        private static Guid updatedQuestionId;
        private static Guid actorId;
    }
}

