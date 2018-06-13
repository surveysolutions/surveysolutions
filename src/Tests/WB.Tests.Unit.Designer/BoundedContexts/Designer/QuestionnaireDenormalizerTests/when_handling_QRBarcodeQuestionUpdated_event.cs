using System;
using System.Collections.Generic;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
using WB.Core.SharedKernels.QuestionnaireEntities;

using it = Moq.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireDenormalizerTests
{
    internal class when_handling_QRBarcodeQuestionUpdated_event : QuestionnaireDenormalizerTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
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
                new CommonQuestionParameters() {Title = title,EnablementCondition = condition,Instructions = instructions, VariableName = variableName},
                null,null, QuestionScope.Interviewer, new List<ValidationCondition>());

            denormalizer = CreateQuestionnaireDenormalizer(questionnaire: questionnaireView);
            BecauseOf();
        }

        private void BecauseOf() {
            denormalizer.UpdateQRBarcodeQuestion(command);
            qRBarcodeQuestion = GetQRBarcodeQuestionById();
        }
        
        [NUnit.Framework.Test] public void should__not_be_null_qr_barcode_question_from_questionnaire__ () =>
            qRBarcodeQuestion.Should().NotBeNull();

        [NUnit.Framework.Test] public void should_set_questionId_as_default_value_for__PublicKey__field () =>
           qRBarcodeQuestion.PublicKey.Should().Be(questionId);

        [NUnit.Framework.Test] public void should_parent_group_exists_in_questionnaire () =>
           questionnaireView.Find<IGroup>(parentGroupId).Should().NotBeNull();

        [NUnit.Framework.Test] public void should_parent_group_contains_qr_barcode_question () =>
           questionnaireView.Find<IGroup>(parentGroupId).Children[0].PublicKey.Should().Be(questionId);

        [NUnit.Framework.Test] public void should_set_null_as_default_value_for__ValidationExpression__field () =>
           qRBarcodeQuestion.ValidationExpression.Should().BeNull();

        [NUnit.Framework.Test] public void should_set_null_as_default_value_for__ValidationMessage__field () =>
            qRBarcodeQuestion.ValidationMessage.Should().BeNull();

        [NUnit.Framework.Test] public void should_set_Interviewer_as_default_value_for__QuestionScope__field () =>
            qRBarcodeQuestion.QuestionScope.Should().Be(QuestionScope.Interviewer);

        [NUnit.Framework.Test] public void should_set_false_as_default_value_for__Featured__field () =>
            qRBarcodeQuestion.Featured.Should().BeFalse();

        [NUnit.Framework.Test] public void should_set_QRBarcode_as_default_value_for__QuestionType__field () =>
            qRBarcodeQuestion.QuestionType.Should().Be(QuestionType.QRBarcode);

        [NUnit.Framework.Test] public void should_set_varibleName_as_value_for__StataExportCaption__field () =>
            qRBarcodeQuestion.StataExportCaption.Should().Be(variableName);

        [NUnit.Framework.Test] public void should_set_title_as_value_for__QuestionText__field () =>
            qRBarcodeQuestion.QuestionText.Should().Be(title);

        [NUnit.Framework.Test] public void should_set_instructions_as_value_for__Instructions__field () =>
            qRBarcodeQuestion.Instructions.Should().Be(instructions);

        [NUnit.Framework.Test] public void should_set_condition_value_for__ConditionExpression__field () =>
            qRBarcodeQuestion.ConditionExpression.Should().Be(condition);

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