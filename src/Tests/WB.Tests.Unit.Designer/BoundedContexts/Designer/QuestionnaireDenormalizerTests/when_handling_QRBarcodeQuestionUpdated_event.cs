using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
using WB.Core.SharedKernels.QuestionnaireEntities;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireDenormalizerTests
{
    internal class when_handling_QRBarcodeQuestionUpdated_event : QuestionnaireDenormalizerTestsContext
    {
        Establish context = () =>
        {
            questionnaireView = CreateQuestionnaireDocument(new[]
            {
                CreateGroup(groupId: parentGroupId,
                    children: new IComposite[]
                    {
                        new QRBarcodeQuestion()
                        { 
                            PublicKey = questionId, 
                            StataExportCaption = "old_var_name",
                            QuestionText = "old title",
                            ConditionExpression = "old condition",
                            Instructions = "old instructions"
                        }
                    })
            }, createdBy: userId);

            command = new UpdateQRBarcodeQuestion(questionnaireView.PublicKey,questionId, userId, 
                new CommonQuestionParameters() {Title = title,EnablementCondition = condition,Instructions = instructions,VariableName = variableName},
                null,null, QuestionScope.Interviewer, new List<ValidationCondition>());

            denormalizer = CreateQuestionnaireDenormalizer(questionnaire: questionnaireView);
        };

        private Because of = () => {
            denormalizer.UpdateQRBarcodeQuestion(command);
            qRBarcodeQuestion = GetQRBarcodeQuestionById();
        };
        
        It should__not_be_null_qr_barcode_question_from_questionnaire__ = ()=>
            qRBarcodeQuestion.ShouldNotBeNull();

        It should_set_questionId_as_default_value_for__PublicKey__field = () =>
           qRBarcodeQuestion.PublicKey.ShouldEqual(questionId);

        It should_parent_group_exists_in_questionnaire = () =>
           questionnaireView.Find<IGroup>(parentGroupId).ShouldNotBeNull();

        It should_parent_group_contains_qr_barcode_question = () =>
           questionnaireView.Find<IGroup>(parentGroupId).Children[0].PublicKey.ShouldEqual(questionId);

        It should_set_null_as_default_value_for__ValidationExpression__field = () =>
           qRBarcodeQuestion.ValidationExpression.ShouldBeNull();

        It should_set_null_as_default_value_for__ValidationMessage__field = () =>
            qRBarcodeQuestion.ValidationMessage.ShouldBeNull();

        It should_set_Interviewer_as_default_value_for__QuestionScope__field = () =>
            qRBarcodeQuestion.QuestionScope.ShouldEqual(QuestionScope.Interviewer);

        It should_set_false_as_default_value_for__Featured__field = () =>
            qRBarcodeQuestion.Featured.ShouldBeFalse();

        It should_set_QRBarcode_as_default_value_for__QuestionType__field = () =>
            qRBarcodeQuestion.QuestionType.ShouldEqual(QuestionType.QRBarcode);

        It should_set_varibleName_as_value_for__StataExportCaption__field = () =>
            qRBarcodeQuestion.StataExportCaption.ShouldEqual(variableName);

        It should_set_title_as_value_for__QuestionText__field = () =>
            qRBarcodeQuestion.QuestionText.ShouldEqual(title);

        It should_set_instructions_as_value_for__Instructions__field = () =>
            qRBarcodeQuestion.Instructions.ShouldEqual(instructions);

        It should_set_condition_value_for__ConditionExpression__field = () =>
            qRBarcodeQuestion.ConditionExpression.ShouldEqual(condition);

        private static IQRBarcodeQuestion GetQRBarcodeQuestionById()
        {
            return questionnaireView.FirstOrDefault<IQRBarcodeQuestion>(question => question.PublicKey == questionId);
        }

        private static IQRBarcodeQuestion qRBarcodeQuestion;
        private static QuestionnaireDocument questionnaireView;
        private static Questionnaire denormalizer;
        private static UpdateQRBarcodeQuestion command;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid parentGroupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid userId = Guid.Parse("A1111111111111111111111111111111");
        private static string variableName = "qr_barcode_question";
        private static string title = "title";
        private static string instructions = "intructions";
        private static string condition = "condition";
    }
}