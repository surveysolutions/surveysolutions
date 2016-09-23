using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.UpdateMultimediaQuestionHandlerTests
{
    internal class when_updating_multimedia_question_and_all_parameters_is_specified : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.AddQuestion(Create.Event.NewQuestionAdded(
                questionType : QuestionType.Text,
                publicKey : questionId,
                groupPublicKey : chapterId,
                questionText : "old title",
                stataExportCaption : "old_variable_name",
                instructions : "old instructions",
                conditionExpression : "old condition",
                responsibleId : responsibleId
            ));
        };

        Because of = () =>
                questionnaire.UpdateMultimediaQuestion(questionId: questionId, title: "title",
                    variableName: "multimedia_question",
                    variableLabel: variableName, enablementCondition: condition, hideIfDisabled: hideIfDisabled, instructions: instructions,
                    responsibleId: responsibleId, scope: QuestionScope.Interviewer, properties: Create.QuestionProperties());



        It should_contains_question_with_QuestionId_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId)
                .PublicKey.ShouldEqual(questionId);

        It should_contains_question_with_variable_name_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId)
                .VariableLabel.ShouldEqual(variableName);

        It should_contains_question_with_title_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId)
                .QuestionText.ShouldEqual(title);

        It should_contains_question_with_condition_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId)
                .ConditionExpression.ShouldEqual(condition);

        It should_contains_question_with_hideIfDisabled_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId)
                .HideIfDisabled.ShouldEqual(hideIfDisabled);

        It should_contains_question_with_instructions_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId)
                .Instructions.ShouldEqual(instructions);

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
