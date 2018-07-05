using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using QuestionnaireVerifier = WB.Core.BoundedContexts.Designer.Verifier.QuestionnaireVerifier;

namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    internal class when_questionnaire_contains_2_gps_prefilled_questions : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = Create.QuestionnaireDocument(
                children: new[]{Create.GpsCoordinateQuestion(questionId: gpsQuestion1, variable:"gps1", isPrefilled:true),
                Create.GpsCoordinateQuestion(questionId: gpsQuestion2, variable: "gps2", isPrefilled:true)}
                );
            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_1_message () =>
            verificationMessages.Count().Should().Be(1);

        [NUnit.Framework.Test] public void should_return_message_with_code_WB0006 () =>
            verificationMessages.First().Code.Should().Be("WB0006");

        [NUnit.Framework.Test] public void should_return_message_with_two_references () =>
            verificationMessages.First().References.Count().Should().Be(2);

        [NUnit.Framework.Test] public void should_return_message_with_first_reference_with_Question_type () =>
            verificationMessages.First().References.First().Type.Should().Be(QuestionnaireVerificationReferenceType.Question);

        [NUnit.Framework.Test] public void should_return_message_with_first_reference_with_id_equals_gpsQuestion1 () =>
            verificationMessages.First().References.First().Id.Should().Be(gpsQuestion1);

        [NUnit.Framework.Test] public void should_return_message_with_second_reference_with_Question_type () =>
        verificationMessages.First().References.Last().Type.Should().Be(QuestionnaireVerificationReferenceType.Question);

        [NUnit.Framework.Test] public void should_return_message_with_second_reference_with_id_equals_gpsQuestion2 () =>
            verificationMessages.First().References.Last().Id.Should().Be(gpsQuestion2);

        static QuestionnaireDocument questionnaire;
        static Guid gpsQuestion1 = Guid.Parse("99999999999999999999999999999999");
        static Guid gpsQuestion2 = Guid.Parse("88888888888888888888888888888888");
        static QuestionnaireVerifier verifier;
        static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
    }
}