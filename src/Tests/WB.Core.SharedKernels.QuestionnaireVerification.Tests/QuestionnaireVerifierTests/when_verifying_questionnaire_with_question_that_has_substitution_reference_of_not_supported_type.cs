using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.QuestionnaireVerification.Implementation.Services;
using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects;

namespace WB.Core.SharedKernels.QuestionnaireVerification.Tests.QuestionnaireVerifierTests
{
    internal class when_verifying_questionnaire_with_question_that_has_substitution_reference_of_not_supported_type : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionWithSubstitutionReferenceToNotSupportedTypeId = Guid.Parse("10000000000000000000000000000000");
            questionSubstitutionReferencerOfNotSupportedTypeId = Guid.Parse("13333333333333333333333333333333");
            questionnaire = CreateQuestionnaireDocument();

            questionnaire.Children.Add(new MultyOptionsQuestion()
            {
                PublicKey = questionSubstitutionReferencerOfNotSupportedTypeId,
                StataExportCaption = unsupported,
                QuestionType = QuestionType.MultyOption
            });

            questionnaire.Children.Add(new SingleQuestion()
            {
                PublicKey = questionWithSubstitutionReferenceToNotSupportedTypeId,
                QuestionText = string.Format("hello %{0}%!", unsupported)
            });

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        It should_return_1_error = () =>
            resultErrors.Count().ShouldEqual(1);

        It should_return_error_with_code__WB0018 = () =>
            resultErrors.Single().Code.ShouldEqual("WB0018");

        It should_return_error_with_2_references = () =>
            resultErrors.Single().References.Count().ShouldEqual(2);

        It should_return_firts_error_reference_with_type_Question = () =>
            resultErrors.Single().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_firts_error_reference_with_id_of_questionWithNotExistingSubstitutionsId = () =>
            resultErrors.Single().References.First().Id.ShouldEqual(questionWithSubstitutionReferenceToNotSupportedTypeId);

        It should_return_last_error_reference_with_type_Question = () =>
            resultErrors.Single().References.Last().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_last_error_reference_with_id_of_questionSubstitutionReferencerOfNotSupportedTypeId = () =>
            resultErrors.Single().References.Last().Id.ShouldEqual(questionSubstitutionReferencerOfNotSupportedTypeId);

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid questionWithSubstitutionReferenceToNotSupportedTypeId;
        private static Guid questionSubstitutionReferencerOfNotSupportedTypeId;
        private const string unsupported = "unsupported";
    }
}
