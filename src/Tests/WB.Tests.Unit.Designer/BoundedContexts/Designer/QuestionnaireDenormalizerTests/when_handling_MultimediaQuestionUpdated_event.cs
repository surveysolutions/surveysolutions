using System;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.SharedKernels.QuestionnaireEntities;


namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireDenormalizerTests
{
    internal class when_handling_MultimediaQuestionUpdated_event : QuestionnaireDenormalizerTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaireView = CreateQuestionnaireDocument(new[]
            {
                CreateGroup(groupId: parentGroupId,
                    children: new IComposite[]
                    {
                        new MultimediaQuestion()
                        { 
                            PublicKey = questionId, 
                            StataExportCaption = "old_var_name",
                            QuestionText = "old title",
                            ConditionExpression = "old condition",
                            Instructions = "old instructions"
                        }
                    })
            }, responsibleId);

            denormalizer = CreateQuestionnaireDenormalizer(questionnaire: questionnaireView);
            BecauseOf();
        }

        private void BecauseOf() =>
            denormalizer.UpdateMultimediaQuestion(questionId, title, variableName, instructions:instructions, enablementCondition:condition, 
                variableLabel:null, hideIfDisabled:false, responsibleId:responsibleId, scope:QuestionScope.Interviewer, properties:new QuestionProperties(false, false));

        [NUnit.Framework.Test] public void should__not_be_null_qr_barcode_question_from_questionnaire__ () =>
            GetMultimediaQuestionById().ShouldNotBeNull();

        [NUnit.Framework.Test] public void should_set_questionId_as_default_value_for__PublicKey__field () =>
           GetMultimediaQuestionById().PublicKey.ShouldEqual(questionId);

        [NUnit.Framework.Test] public void should_parent_group_exists_in_questionnaire () =>
           questionnaireView.Find<IGroup>(parentGroupId).ShouldNotBeNull();

        [NUnit.Framework.Test] public void should_parent_group_contains_qr_barcode_question () =>
           questionnaireView.Find<IGroup>(parentGroupId).Children[0].PublicKey.ShouldEqual(questionId);

        [NUnit.Framework.Test] public void should_set_null_as_default_value_for__ValidationExpression__field () =>
           GetMultimediaQuestionById().ValidationExpression.ShouldBeNull();

        [NUnit.Framework.Test] public void should_set_null_as_default_value_for__ValidationMessage__field () =>
            GetMultimediaQuestionById().ValidationMessage.ShouldBeNull();

        [NUnit.Framework.Test] public void should_set_Interviewer_as_default_value_for__QuestionScope__field () =>
            GetMultimediaQuestionById().QuestionScope.ShouldEqual(QuestionScope.Interviewer);

        [NUnit.Framework.Test] public void should_set_false_as_default_value_for__Featured__field () =>
            GetMultimediaQuestionById().Featured.ShouldBeFalse();

        [NUnit.Framework.Test] public void should_set_Multimedia_as_default_value_for__QuestionType__field () =>
            GetMultimediaQuestionById().QuestionType.ShouldEqual(QuestionType.Multimedia);

        [NUnit.Framework.Test] public void should_set_varibleName_as_value_for__StataExportCaption__field () =>
            GetMultimediaQuestionById().StataExportCaption.ShouldEqual(variableName);

        [NUnit.Framework.Test] public void should_set_title_as_value_for__QuestionText__field () =>
            GetMultimediaQuestionById().QuestionText.ShouldEqual(title);

        [NUnit.Framework.Test] public void should_set_instructions_as_value_for__Instructions__field () =>
            GetMultimediaQuestionById().Instructions.ShouldEqual(instructions);

        [NUnit.Framework.Test] public void should_set_condition_value_for__ConditionExpression__field () =>
            GetMultimediaQuestionById().ConditionExpression.ShouldEqual(condition);

        private static IMultimediaQuestion GetMultimediaQuestionById()
        {
            return questionnaireView.FirstOrDefault<IMultimediaQuestion>(question => question.PublicKey == questionId);
        }

        private static QuestionnaireDocument questionnaireView;
        private static Questionnaire denormalizer;
        private static Guid responsibleId = Guid.Parse("B1111111111111111111111111111111");
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid parentGroupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static string variableName = "qr_barcode_question";
        private static string title = "title";
        private static string instructions = "intructions";
        private static string condition = "condition";
    }
}
