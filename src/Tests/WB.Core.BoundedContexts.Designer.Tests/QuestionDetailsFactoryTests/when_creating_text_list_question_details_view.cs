using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionDetailsFactoryTests
{
    internal class when_creating_text_list_question_details_view : QuestionDetailsFactoryTestContext
    {
        Establish context = () =>
        {
            question = new TextListQuestion
            {
                PublicKey = questionId,
                QuestionType = questionType,
                QuestionText = title,
                ConditionExpression = enablementCondition,
                ValidationExpression = validationExpression,
                ValidationMessage = validationMessage,
                Featured = isPrefilled,
                Mandatory = isMandatory,
                StataExportCaption = variableName,
                Instructions = instructions,
                QuestionScope = scope,
                MaxAnswerCount = maxAnswerCount
            };

            viewMapper = CreateQuestionDetailsFactory();
        };

        Because of = () =>
            questionView = (TextListDetailsView)viewMapper.Map(question, parentGroupId);

        It should_set_Type_in_DateTime = () =>
            questionView.Type.ShouldEqual(questionType);

        It should_set_Idt_in_questionId = () =>
            questionView.Id.ShouldEqual(questionId);

        It should_set_ParentGroupsId_in_parentGroupId = () =>
            questionView.ParentGroupId.ShouldEqual(parentGroupId);

        It should_set_empty_RosterScopeIds = () =>
            questionView.RosterScopeIds.ShouldBeEmpty();

        It should_set_empty_ParentGroupsIds = () =>
            questionView.ParentGroupsIds.ShouldBeEmpty();

        It should_set_EnablementConditiont_in_enablementCondition = () =>
            questionView.EnablementCondition.ShouldEqual(enablementCondition);

        It should_set_IsPreFilledt_in_isPrefilled = () =>
            questionView.IsPreFilled.ShouldEqual(isPrefilled);

        It should_set_Instructionst_in_instructions = () =>
            questionView.Instructions.ShouldEqual(instructions);

        It should_set_IsMandatoryt_in_isMandatory = () =>
            questionView.IsMandatory.ShouldEqual(isMandatory);

        It should_set_QuestionScopet_in_scope = () =>
            questionView.QuestionScope.ShouldEqual(scope);

        It should_set_VariableNamet_in_variableName = () =>
            questionView.VariableName.ShouldEqual(variableName);

        It should_set_Titlet_in_title = () =>
            questionView.Title.ShouldEqual(title);

        It should_set_ValidationExpressiont_in_validationExpression = () =>
            questionView.ValidationExpression.ShouldEqual(validationExpression);

        It should_set_ValidationMessaget_in_validationMessage = () =>
            questionView.ValidationMessage.ShouldEqual(validationMessage);

        private static IQuestion question;
        private static Guid parentGroupId = Guid.Parse("11111111111111111111111111111111");
        private static Guid questionId = Guid.Parse("22222222222222222222222222222222");
        private static QuestionType questionType = QuestionType.TextList;
        private static string title = "some title";
        private static string enablementCondition = "some condition";
        private static string validationExpression = "expresstion";
        private static string validationMessage = "message";
        private static bool isPrefilled = true;
        private static string instructions = "message";
        private static bool isMandatory = true;
        private static QuestionScope scope = QuestionScope.Supervisor;
        private static string variableName = "variable";

        private static QuestionDetailsViewMapper viewMapper;
        private static TextListDetailsView questionView;
        private static readonly int maxAnswerCount = 14;
    }
}