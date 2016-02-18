using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    [Ignore("KP-4518")]
    internal class when_verifying_questionnaire_with_question_that_has_custom_validation_referencing_question_with_deeper_roster_level : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionWithCustomValidation = Guid.Parse("10000000000000000000000000000000");
            underDeeperRosterLevelQuestionId = Guid.Parse("12222222222222222222222222222222");
            var rosterGroupId = Guid.Parse("13333333333333333333333333333333");
            var rosterQuestionId = Guid.Parse("13333333333333333333333333333333");
            questionnaire = CreateQuestionnaireDocument();

            questionnaire.Children.Add(new NumericQuestion
            {
                PublicKey = rosterGroupId,
                StataExportCaption = "var1",
                IsInteger = true
            });

            var rosterGroup = new Group() { PublicKey = rosterGroupId, VariableName = "a", IsRoster = true, RosterSizeQuestionId = rosterQuestionId };
            rosterGroup.Children.Add(new NumericQuestion() { PublicKey = underDeeperRosterLevelQuestionId, StataExportCaption = "var2" });
            questionnaire.Children.Add(rosterGroup);
            questionnaire.Children.Add(new SingleQuestion()
            {
                PublicKey = questionWithCustomValidation,
                ValidationExpression = "some random expression",
                ValidationMessage = "some random message",
                StataExportCaption = "var3",
                Answers = { new Answer() { AnswerValue = "1", AnswerText = "opt 1" }, new Answer() { AnswerValue = "2", AnswerText = "opt 2" } }
            });

            var expressionProcessor = new Mock<IExpressionProcessor>();

            expressionProcessor.Setup(x => x.GetIdentifiersUsedInExpression(Moq.It.IsAny<string>()))
                .Returns(new string[] { underDeeperRosterLevelQuestionId.ToString() });

            verifier = CreateQuestionnaireVerifier(expressionProcessor.Object);
        };

        Because of = () =>
            verificationMessages = verifier.CheckForErrors(questionnaire);

        It should_return_1_message = () =>
            verificationMessages.Count().ShouldEqual(1);

        It should_return_message_with_code__WB0014 = () =>
            verificationMessages.Single().Code.ShouldEqual("WB0014");

        It should_return_message_with_two_references = () =>
            verificationMessages.Single().References.Count().ShouldEqual(2);

        It should_return_first_message_reference_with_type_Question = () =>
            verificationMessages.Single().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_first_message_reference_with_id_of_questionWithCustomValidation = () =>
            verificationMessages.Single().References.First().Id.ShouldEqual(questionWithCustomValidation);

        It should_return_last_message_reference_with_type_Question = () =>
            verificationMessages.Single().References.Last().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_last_message_reference_with_id_of_underDeeperPropagationLevelQuestionId = () =>
            verificationMessages.Single().References.Last().Id.ShouldEqual(underDeeperRosterLevelQuestionId);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid questionWithCustomValidation;
        private static Guid underDeeperRosterLevelQuestionId;
    }
}
