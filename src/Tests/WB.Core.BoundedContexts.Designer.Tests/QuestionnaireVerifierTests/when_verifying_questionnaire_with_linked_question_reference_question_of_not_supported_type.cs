using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects.Verification;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireVerifierTests
{
    internal class when_verifying_questionnaire_with_linked_question_reference_question_of_not_supported_type : QuestionnaireVerifierTestsContext
    {
        private Establish context = () =>
        {
            linkedQuestionId = Guid.Parse("10000000000000000000000000000000");
            notSupportedForLinkingQuestionId = Guid.Parse("13333333333333333333333333333333");
            questionnaire = CreateQuestionnaireDocument();

            questionnaire.Children.Add(new SingleQuestion()
            {
                PublicKey = notSupportedForLinkingQuestionId,
                QuestionType = QuestionType.SingleOption
            });

            questionnaire.Children.Add(new SingleQuestion() { PublicKey = linkedQuestionId, LinkedToQuestionId = notSupportedForLinkingQuestionId });
            verifier = CreateQuestionnaireVerifier();
        };

        private Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        private It should_return_1_error = () =>
            resultErrors.Count().ShouldEqual(1);

        private It should_return_error_with_code__WB0012__ = () =>
            resultErrors.Single().Code.ShouldEqual("WB0012");

        private It should_return_error_with_one_references = () =>
            resultErrors.Single().References.Count().ShouldEqual(1);

        private It should_return_error_reference_with_type_Question = () =>
            resultErrors.Single().References.Single().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        private It should_return_error_reference_with_id_of_linkedQuestionId = () =>
            resultErrors.Single().References.Single().Id.ShouldEqual(linkedQuestionId);

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid linkedQuestionId;
        private static Guid notSupportedForLinkingQuestionId;
    }
}
