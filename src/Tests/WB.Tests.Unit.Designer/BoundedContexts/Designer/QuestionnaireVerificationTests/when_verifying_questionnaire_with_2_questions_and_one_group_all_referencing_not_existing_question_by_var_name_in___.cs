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

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    [Ignore("reference validation is turned off")]
    internal class when_verifying_questionnaire_with_2_questions_and_one_group_all_referencing_not_existing_question_by_var_name_in_enablement_condition :
            QuestionnaireVerifierTestsContext
    {
        private [NUnit.Framework.OneTimeSetUp] public void context () {
            const string EnablementConditionWithNotExistingQuestion = "[99999999999999999999999999999999] == 2";

            firstIncorrectQuestionId = Guid.Parse("11111111111111111111111111111111");
            secondIncorrectQuestionId = Guid.Parse("22222222222222222222222222222222");
            textQuestionId = Guid.Parse("a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1");

            incorrectGroupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                new NumericQuestion
                {
                    PublicKey = firstIncorrectQuestionId,
                    ConditionExpression = EnablementConditionWithNotExistingQuestion,
                    StataExportCaption = "var1"
                },
                new NumericQuestion
                {
                    PublicKey = secondIncorrectQuestionId,
                    ConditionExpression = EnablementConditionWithNotExistingQuestion,
                    StataExportCaption = "var2"
                },
                new Group { PublicKey = incorrectGroupId, ConditionExpression = EnablementConditionWithNotExistingQuestion },
                new TextQuestion { PublicKey = textQuestionId, StataExportCaption = "var3" },
                new Group { PublicKey = Guid.NewGuid() }
                );

            var expressionProcessor = Mock.Of<IExpressionProcessor>(processor
                => processor.GetIdentifiersUsedInExpression(EnablementConditionWithNotExistingQuestion) ==
                        new[] { "notExistingVariableName" });

            verifier = CreateQuestionnaireVerifier(expressionProcessor: expressionProcessor);
        }

        private void BecauseOf() =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_3_messages () =>
            verificationMessages.Count().ShouldEqual(3);

        [NUnit.Framework.Test] public void should_return_messages_each_with_code__WB0005__ () =>
            verificationMessages.ShouldEachConformTo(error
                => error.Code == "WB0005");

        [NUnit.Framework.Test] public void should_return_messages_each_having_single_reference () =>
            verificationMessages.ShouldEachConformTo(error
                => error.References.Count() == 1);

        [NUnit.Framework.Test] public void should_return_message_referencing_first_incorrect_question () =>
            verificationMessages.ShouldContain(error
                => error.References.Single().Type == QuestionnaireVerificationReferenceType.Question
                    && error.References.Single().Id == firstIncorrectQuestionId);

        [NUnit.Framework.Test] public void should_return_message_referencing_second_incorrect_question () =>
            verificationMessages.ShouldContain(error
                => error.References.Single().Type == QuestionnaireVerificationReferenceType.Question
                    && error.References.Single().Id == secondIncorrectQuestionId);

        [NUnit.Framework.Test] public void should_return_message_referencing_incorrect_group () =>
            verificationMessages.ShouldContain(error
                => error.References.Single().Type == QuestionnaireVerificationReferenceType.Group
                    && error.References.Single().Id == incorrectGroupId);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid firstIncorrectQuestionId;
        private static Guid secondIncorrectQuestionId;
        private static Guid textQuestionId;

        private static Guid incorrectGroupId;
    }
}
