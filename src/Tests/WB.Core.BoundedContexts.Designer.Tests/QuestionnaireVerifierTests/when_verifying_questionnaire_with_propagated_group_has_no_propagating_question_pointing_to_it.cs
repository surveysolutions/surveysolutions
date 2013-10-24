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
    internal class when_verifying_questionnaire_with_propagated_group_has_no_propagating_question_pointing_to_it : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            autopropagatedGroupId = Guid.Parse("10000000000000000000000000000000");
            questionnaire = CreateQuestionnaireDocument();
            questionnaire.Children.Add(new TextQuestion("random question"));
            questionnaire.Children.Add(new Group() { PublicKey = autopropagatedGroupId, Propagated = Propagate.AutoPropagated });
            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        It should_return_1_error = () =>
            resultErrors.Count().ShouldEqual(1);

        It should_return_error_with_code__WB0009__ = () =>
            resultErrors.Single().Code.ShouldEqual("WB0009");

        It should_return_error_with_one_references = () =>
            resultErrors.Single().References.Count().ShouldEqual(1);

        It should_return_error_reference_with_type_group = () =>
            resultErrors.Single().References.Single().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Group);

        It should_return_error_reference_with_id_of_autopropagatedGroupId = () =>
            resultErrors.Single().References.Single().Id.ShouldEqual(autopropagatedGroupId);

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid autopropagatedGroupId;
    }
}
