using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.QuestionnaireVerification.Implementation.Services;
using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects;

namespace WB.Core.SharedKernels.QuestionnaireVerification.Tests.QuestionnaireVerifierTests
{
    internal class when_verifying_questionnaire_with_questioWn_with_substitutions_inside_autoropagated_unattached_group : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionWithSubstitutionsId = Guid.Parse("10000000000000000000000000000000");
            underDeeperPropagationLevelQuestionId = Guid.Parse("12222222222222222222222222222222");
            autoPropagatedGroupId = Guid.Parse("13333333333333333333333333333333");
            questionnaire = CreateQuestionnaireDocument();

            var autopropagatedGroup = new Group() { PublicKey = autoPropagatedGroupId, Propagated = Propagate.AutoPropagated };

            autopropagatedGroup.Children.Add(new NumericQuestion()
            {
                PublicKey = underDeeperPropagationLevelQuestionId,
                StataExportCaption = underDeeperPropagationLevelQuestionVariableName
            });
            questionnaire.Children.Add(autopropagatedGroup);
            questionnaire.Children.Add(new SingleQuestion()
            {
                PublicKey = questionWithSubstitutionsId,
                QuestionText = string.Format("hello %{0}%", underDeeperPropagationLevelQuestionVariableName)
            });

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        It should_return_2_errors = () =>
            resultErrors.Count().ShouldEqual(2);

        It should_return_first_error_with_code__WB0009 = () =>
            resultErrors.First().Code.ShouldEqual("WB0009");

        It should_return_second_error_with_code__WB0019 = () =>
            resultErrors.Last().Code.ShouldEqual("WB0019");

        It should_return_WB0019_error_with_two_references = () =>
            resultErrors.Last().References.Count().ShouldEqual(2);

        It should_return_WB0019_first_error_reference_with_type_Question = () =>
            resultErrors.Last().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_WB0019_first_error_reference_with_id_of_underDeeperPropagationLevelQuestionId = () =>
            resultErrors.Last().References.First().Id.ShouldEqual(questionWithSubstitutionsId);

        It should_return_WB0019_last_error_reference_with_type_Question = () =>
            resultErrors.Last().References.Last().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_WB0019_last_error_reference_with_id_of_underDeeperPropagationLevelQuestionVariableName = () =>
            resultErrors.Last().References.Last().Id.ShouldEqual(underDeeperPropagationLevelQuestionId);

        It should_return_WB0009_error_with_single_referenceon_group = () =>
            resultErrors.First().References.Single().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Group);

        It should_return_WB0009_error_with_single_reference_with_id_of_autopropagatedGroupId = () =>
            resultErrors.First().References.Single().Id.ShouldEqual(autoPropagatedGroupId);

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid questionWithSubstitutionsId;
        private static Guid underDeeperPropagationLevelQuestionId;
        private static Guid autoPropagatedGroupId;
        private const string underDeeperPropagationLevelQuestionVariableName = "i_am_deeper_ddddd_deeper";
    }
}
