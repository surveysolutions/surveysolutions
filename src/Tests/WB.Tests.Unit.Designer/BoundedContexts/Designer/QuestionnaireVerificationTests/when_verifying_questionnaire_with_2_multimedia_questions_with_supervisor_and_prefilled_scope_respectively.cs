using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_2_multimedia_questions_with_supervisor_and_prefilled_scope_respectively : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = CreateQuestionnaireDocument(Create.MultimediaQuestion(
                supervisorQuestionId,
                scope: QuestionScope.Supervisor,
                variable: "var1"
            ),
            Create.MultimediaQuestion(
                hqQuestionId,
                scope: QuestionScope.Headquarter,
                variable: "var2"
            ));

            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_2_messages () =>
            verificationMessages.Count().ShouldEqual(2);

        [NUnit.Framework.Test] public void should_return_first_error_with_code__WB0078 () =>
            verificationMessages.First().Code.ShouldEqual("WB0078");

        [NUnit.Framework.Test] public void should_return_message_with_level_general () =>
            verificationMessages.First().MessageLevel.ShouldEqual(VerificationMessageLevel.General);

        [NUnit.Framework.Test] public void should_return_first_error_with_1_references () =>
            verificationMessages.First().References.Count().ShouldEqual(1);

        [NUnit.Framework.Test] public void should_return_first_message_reference_with_type_Question () =>
            verificationMessages.First().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        [NUnit.Framework.Test] public void should_return_first_message_reference_with_id_of_supervisorQuestionId () =>
            verificationMessages.First().References.First().Id.ShouldEqual(supervisorQuestionId);

        [NUnit.Framework.Test] public void should_return_second_error_with_code__WB0078 () =>
            verificationMessages.Last().Code.ShouldEqual("WB0078");

        [NUnit.Framework.Test] public void should_return_second_error_with_1_references () =>
            verificationMessages.Last().References.Count().ShouldEqual(1);

        [NUnit.Framework.Test] public void should_return_second_message_reference_with_type_Question () =>
            verificationMessages.Last().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        [NUnit.Framework.Test] public void should_return_second_message_reference_with_id_of_hqQuestionId () =>
            verificationMessages.Last().References.First().Id.ShouldEqual(hqQuestionId);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid supervisorQuestionId = Guid.Parse("10000000000000000000000000000000");
        private static Guid hqQuestionId = Guid.Parse("20000000000000000000000000000000");
    }
}
