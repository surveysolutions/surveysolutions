using System;
using FluentAssertions;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;

using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.UpdateMultimediaQuestionHandlerTests
{
    internal class when_updating_multimedia_question_and_all_parameters_is_specified : QuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
            questionnaire.AddTextQuestion(
                questionId,
                chapterId,
                title: "old title",
                variableName: "old_variable_name",
                instructions : "old instructions",
                enablementCondition : "old condition",
                responsibleId : responsibleId
            );
            BecauseOf();
        }

        private void BecauseOf() =>
            questionnaire.UpdateMultimediaQuestion(
                Create.Command.UpdateMultimediaQuestion(
                    questionId: questionId, title: "title",
                    variableName: "multimedia_question",
                    variableLabel: variableName, enablementCondition: condition, hideIfDisabled: hideIfDisabled,
                    instructions: instructions,
                    responsibleId: responsibleId, scope: QuestionScope.Interviewer,
                    properties: Create.QuestionProperties(),
                    isSignature: false));

        [NUnit.Framework.Test] public void should_contains_question_with_QuestionId_specified () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId)
                .PublicKey.Should().Be(questionId);

        [NUnit.Framework.Test] public void should_contains_question_with_variable_name_specified () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId)
                .VariableLabel.Should().Be(variableName);

        [NUnit.Framework.Test] public void should_contains_question_with_title_specified () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId)
                .QuestionText.Should().Be(title);

        [NUnit.Framework.Test] public void should_contains_question_with_condition_specified () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId)
                .ConditionExpression.Should().Be(condition);

        [NUnit.Framework.Test] public void should_contains_question_with_hideIfDisabled_specified () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId)
                .HideIfDisabled.Should().Be(hideIfDisabled);

        [NUnit.Framework.Test] public void should_contains_question_with_instructions_specified () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId)
                .Instructions.Should().Be(instructions);

        private static Questionnaire questionnaire;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static string variableName = "multimedia_question";
        private static string title = "title";
        private static string instructions = "intructions";
        private static string condition = "condition";
        private static bool hideIfDisabled = true;
    }
}
