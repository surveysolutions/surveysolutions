using System;
using FluentAssertions;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireDenormalizerTests
{
    internal class when_handling_MultimediaQuestionUpdated_event : QuestionnaireDenormalizerTestsContext
    {
        private static QuestionnaireDocument questionnaireView;
        private static Questionnaire denormalizer;
        private static Guid responsibleId = Guid.Parse("B1111111111111111111111111111111");
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid parentGroupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static string variableName = "qr_barcode_question";
        private static string title = "title";
        private static string instructions = "intructions";
        private static string condition = "condition";

        [OneTimeSetUp]
        public void context()
        {
            questionnaireView = CreateQuestionnaireDocument(new[]
            {
                CreateGroup(parentGroupId,
                    children: new IComposite[]
                    {
                        new MultimediaQuestion
                        {
                            PublicKey = questionId,
                            StataExportCaption = "old_var_name",
                            QuestionText = "old title",
                            ConditionExpression = "old condition",
                            Instructions = "old instructions"
                        }
                    })
            }, responsibleId);

            denormalizer = CreateQuestionnaireDenormalizer(questionnaireView);
            BecauseOf();
        }

        private void BecauseOf()
        {
            denormalizer.UpdateMultimediaQuestion(
                Create.Command.UpdateMultimediaQuestion(
                    questionId,
                    title,
                    variableName,
                    instructions,
                    condition,
                    null,
                    false,
                    responsibleId,
                    QuestionScope.Interviewer,
                    new QuestionProperties(false, false),
                    true));
        }

        private static IMultimediaQuestion GetMultimediaQuestionById()
        {
            return questionnaireView.FirstOrDefault<IMultimediaQuestion>(question => question.PublicKey == questionId);
        }

        [Test]
        public void should__not_be_null_qr_barcode_question_from_questionnaire__()
        {
            GetMultimediaQuestionById().Should().NotBeNull();
        }

        [Test]
        public void should_parent_group_contains_qr_barcode_question()
        {
            questionnaireView.Find<IGroup>(parentGroupId).Children[0].PublicKey.Should().Be(questionId);
        }

        [Test]
        public void should_parent_group_exists_in_questionnaire()
        {
            questionnaireView.Find<IGroup>(parentGroupId).Should().NotBeNull();
        }

        [Test]
        public void should_set_condition_value_for__ConditionExpression__field()
        {
            GetMultimediaQuestionById().ConditionExpression.Should().Be(condition);
        }

        [Test]
        public void should_set_false_as_default_value_for__Featured__field()
        {
            GetMultimediaQuestionById().Featured.Should().BeFalse();
        }

        [Test]
        public void should_set_instructions_as_value_for__Instructions__field()
        {
            GetMultimediaQuestionById().Instructions.Should().Be(instructions);
        }

        [Test]
        public void should_set_Interviewer_as_default_value_for__QuestionScope__field()
        {
            GetMultimediaQuestionById().QuestionScope.Should().Be(QuestionScope.Interviewer);
        }

        [Test]
        public void should_set_is_is_signature_flag()
        {
            GetMultimediaQuestionById().IsSignature.Should().BeTrue();
        }

        [Test]
        public void should_set_Multimedia_as_default_value_for__QuestionType__field()
        {
            GetMultimediaQuestionById().QuestionType.Should().Be(QuestionType.Multimedia);
        }

        [Test]
        public void should_set_empty_list_of_validation_conditions()
        {
            GetMultimediaQuestionById().ValidationConditions.Should().HaveCount(0);
        }

        [Test]
        public void should_set_questionId_as_default_value_for__PublicKey__field()
        {
            GetMultimediaQuestionById().PublicKey.Should().Be(questionId);
        }

        [Test]
        public void should_set_title_as_value_for__QuestionText__field()
        {
            GetMultimediaQuestionById().QuestionText.Should().Be(title);
        }

        [Test]
        public void should_set_varibleName_as_value_for__StataExportCaption__field()
        {
            GetMultimediaQuestionById().StataExportCaption.Should().Be(variableName);
        }
    }
}
