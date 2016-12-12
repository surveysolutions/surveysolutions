using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_questionnaire_contains_2_gps_prefilled_questions : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionnaire = Create.QuestionnaireDocument(
                children: new[]{Create.GpsCoordinateQuestion(questionId: gpsQuestion1, variable:"gps1", isPrefilled:true),
                Create.GpsCoordinateQuestion(questionId: gpsQuestion2, variable: "gps2", isPrefilled:true)}
                );
            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        It should_return_1_message = () =>
            verificationMessages.Count().ShouldEqual(1);

        It should_return_message_with_code_WB0006 = () =>
            verificationMessages.First().Code.ShouldEqual("WB0006");

        It should_return_message_with_two_references = () =>
            verificationMessages.First().References.Count().ShouldEqual(2);

        It should_return_message_with_first_reference_with_Question_type = () =>
            verificationMessages.First().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_message_with_first_reference_with_id_equals_gpsQuestion1 = () =>
            verificationMessages.First().References.First().Id.ShouldEqual(gpsQuestion1);

        It should_return_message_with_second_reference_with_Question_type = () =>
        verificationMessages.First().References.Last().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_message_with_second_reference_with_id_equals_gpsQuestion2 = () =>
            verificationMessages.First().References.Last().Id.ShouldEqual(gpsQuestion2);

        static QuestionnaireDocument questionnaire;
        static Guid gpsQuestion1 = Guid.Parse("99999999999999999999999999999999");
        static Guid gpsQuestion2 = Guid.Parse("88888888888888888888888888888888");
        static QuestionnaireVerifier verifier;
        static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
    }
}