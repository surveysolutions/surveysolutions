using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_TextList_question_that_marked_as_prefilled : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            prefilledTextListquestionId = Guid.Parse("10000000000000000000000000000000");
            textQuestionId = Guid.Parse("20000000000000000000000000000000");
            questionnaire = CreateQuestionnaireDocument(
            Create.TextListQuestion(
                prefilledTextListquestionId,
                variable: "var1",
                featured: true
            ),
            Create.TextListQuestion(
                variable: "var2",
                questionId: textQuestionId
            ));

            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_1_message () =>
            verificationMessages.Count().ShouldEqual(1);

        [NUnit.Framework.Test] public void should_return_message_with_level_general () =>
            verificationMessages.Single().MessageLevel.ShouldEqual(VerificationMessageLevel.General);

        [NUnit.Framework.Test] public void should_return_message_with_code__WB0039 () =>
            verificationMessages.Single().Code.ShouldEqual("WB0039");

        [NUnit.Framework.Test] public void should_return_message_with_1_references () =>
            verificationMessages.Single().References.Count().ShouldEqual(1);

        [NUnit.Framework.Test] public void should_return_message_reference_with_type_Question () =>
            verificationMessages.Single().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        [NUnit.Framework.Test] public void should_return_message_reference_with_id_of_prefilledTextListquestionId () =>
            verificationMessages.Single().References.First().Id.ShouldEqual(prefilledTextListquestionId);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid prefilledTextListquestionId;
        private static Guid textQuestionId;
    }
}
