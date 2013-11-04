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
    internal class when_verifying_questionnaire_with_2_orphan_auto_propagated_groups_they_must_be_treated_as_grops_with_separate_propagation_levels : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {

            autoPropagatedGroupId1 = Guid.Parse("13333333333333333333333333333333");
            autoPropagatedGroupId2 = Guid.Parse("10000000000000000000000000000000");
            questionWithSubstitutionsIdFromLevel1 = Guid.Parse("12222222222222222222222222222222");
            questionFromLevel2 = Guid.Parse("13333333333333333333333333333333");

            questionnaire = CreateQuestionnaireDocument();

            var autopropagatedGroup1 = new Group() { PublicKey = autoPropagatedGroupId1, Propagated = Propagate.AutoPropagated };

            autopropagatedGroup1.Children.Add(new NumericQuestion()
            {
                PublicKey = questionWithSubstitutionsIdFromLevel1,
                QuestionText = string.Format("hello %{0}%", questionSubstitutionsSourceFromLevel2VariableName)
            });
            questionnaire.Children.Add(autopropagatedGroup1);

            var autopropagatedGroup2 = new Group() { PublicKey = autoPropagatedGroupId2, Propagated = Propagate.AutoPropagated };
            autopropagatedGroup2.Children.Add(new SingleQuestion()
            {
                PublicKey = questionFromLevel2,
                StataExportCaption = questionSubstitutionsSourceFromLevel2VariableName
            });
            questionnaire.Children.Add(autopropagatedGroup2);
            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        It should_return_3_errors = () =>
            resultErrors.Count().ShouldEqual(3);

        It should_return_first_error_with_code__WB0009 = () =>
            resultErrors.First().Code.ShouldEqual("WB0009");

        It should_return_second_error_with_code_WB0009 = () =>
            resultErrors.Skip(1).First().Code.ShouldEqual("WB0009");

        It should_return_third_error_with_code__WB0019 = () =>
            resultErrors.Last().Code.ShouldEqual("WB0019");

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid questionWithSubstitutionsIdFromLevel1;
        private static Guid questionFromLevel2;
        private static Guid autoPropagatedGroupId1;
        private static Guid autoPropagatedGroupId2;
        private const string questionSubstitutionsSourceFromLevel2VariableName = "i_am_from_level2";
    }
}
