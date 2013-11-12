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
    internal class when_verifying_questionnaire_with_question_with_substitutions_inside_autoropagated_unattached_group : QuestionnaireVerifierTestsContext
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

        It should_return_1_error = () =>
            resultErrors.Count().ShouldEqual(1);

        It should_return_error_with_code__WB0009 = () =>
            resultErrors.Single().Code.ShouldEqual("WB0009");

        It should_return_WB0009_error_with_single_referenceon_group = () =>
            resultErrors.Single().References.Single().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Group);

        It should_return_WB0009_error_with_single_reference_with_id_of_autopropagatedGroupId = () =>
            resultErrors.Single().References.Single().Id.ShouldEqual(autoPropagatedGroupId);

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid questionWithSubstitutionsId;
        private static Guid underDeeperPropagationLevelQuestionId;
        private static Guid autoPropagatedGroupId;
        private const string underDeeperPropagationLevelQuestionVariableName = "i_am_deeper_ddddd_deeper";
    }
}
